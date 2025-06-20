/*
 * OCPP.Core - https://github.com/dallmann-consulting/OCPP.Core
 * Copyright (C) 2020-2021 dallmann consulting GmbH.
 * All Rights Reserved.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using OCPP.Core.Server.Entities;
using OCPP.Core.Server.Messages_OCPP16;
using OCPP.Core.Server.Models;

namespace OCPP.Core.Server
{
    public partial class ControllerBase
    {
        /// <summary>
        /// Internal string for OCPP protocol version
        /// </summary>
        protected virtual string ProtocolVersion { get;  }

        /// <summary>
        /// Configuration context for reading app settings
        /// </summary>
        protected IConfiguration Configuration { get; set; }

        /// <summary>
        /// Chargepoint status
        /// </summary>
        protected ChargePointStatus ChargePointStatus { get; set; }

        /// <summary>
        /// ILogger object
        /// </summary>
        protected ILogger Logger { get; set; }

        /// <summary>
        /// DbContext object
        /// </summary>
        protected NpgsqlDbContext DbContext { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerBase(IConfiguration config, ILoggerFactory loggerFactory, ChargePointStatus chargePointStatus, NpgsqlDbContext dbContext)
        {
            Configuration = config;

            if (chargePointStatus != null)
            {
                ChargePointStatus = chargePointStatus;
            }
            else
            {
                Logger.LogError("New ControllerBase => empty chargepoint status");
            }
            DbContext = dbContext;
        }

        /// <summary>
        /// Deserialize and validate JSON message (if schema file exists)
        /// </summary>
        protected T DeserializeMessage<T>(OCPPMessage msg)
        {
            string path = Assembly.GetExecutingAssembly().Location;
            string codeBase = Path.GetDirectoryName(path);

            bool validateMessages = Configuration.GetValue<bool>("ValidateMessages", false);

            string schemaJson = null;
            if (validateMessages && 
                !string.IsNullOrEmpty(codeBase) && 
                Directory.Exists(codeBase))
            {
                string msgTypeName = typeof(T).Name;
                string filename = Path.Combine(codeBase, $"Schema{ProtocolVersion}", $"{msgTypeName}.json");
                if (File.Exists(filename))
                {
                    Logger.LogTrace("DeserializeMessage => Using schema file: {0}", filename);
                    schemaJson = File.ReadAllText(filename);
                }
            }

            JsonTextReader reader = new JsonTextReader(new StringReader(msg.JsonPayload));
            JsonSerializer serializer = new JsonSerializer();

            if (!string.IsNullOrEmpty(schemaJson))
            {
                JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(reader);
                validatingReader.Schema = JSchema.Parse(schemaJson);

                IList<string> messages = new List<string>();
                validatingReader.ValidationEventHandler += (o, a) => messages.Add(a.Message);
                T obj = serializer.Deserialize<T>(validatingReader);
                if (messages.Count > 0)
                {
                    foreach (string err in messages)
                    {
                        Logger.LogWarning("DeserializeMessage {0} => Validation error: {1}", msg.Action, err);
                    }
                    throw new FormatException("Message validation failed");
                }
                return obj;
            }
            else
            {
                // Deserialization WITHOUT schema validation
                Logger.LogTrace("DeserializeMessage => Deserialization without schema validation");
                return serializer.Deserialize<T>(reader);
            }
        }


        /// <summary>
        /// Helper function for creating and updating the ConnectorStatus in then database
        /// </summary>
        protected bool UpdateConnectorStatus(int connectorId, string status, DateTimeOffset? statusTime, double? meter, double? chargeRateKW, double? SoC, DateTimeOffset? meterTime)
        {
            try
            {
                var charger = DbContext.TblChargers.Where(x => x.FShortName == ChargePointStatus.Id).FirstOrDefault() ?? new TblCharger();
                var connectorStatus = DbContext.TblConnectorStatuses.Where(x => x.FChargerId == charger.FId && x.FConnectorId == connectorId).FirstOrDefault();                //TblConnectorStatus connectorStatus = DbContext.Find<TblConnectorStatus>(charger.FId, connectorId);
                if (connectorStatus == null)
                {
                    // no matching entry => create connector status
                    connectorStatus = new TblConnectorStatus();
                    connectorStatus.FChargerId = charger.FId;
                    connectorStatus.FConnectorId = connectorId;
                    connectorStatus.FStationId = charger.FStationId;
                    Logger.LogTrace("UpdateConnectorStatus => Creating new DB-ConnectorStatus: ID={0} / Connector={1}", connectorStatus.FChargerId, connectorStatus.FConnectorId);
                    DbContext.Add<TblConnectorStatus>(connectorStatus);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    //var time = ((statusTime.HasValue) ? statusTime.Value : DateTimeOffset.Now).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", new System.Globalization.CultureInfo("en-US"));
                    var time = statusTime.HasValue ? statusTime.Value.UtcDateTime : DateTime.UtcNow;
                    connectorStatus.FCurrentStatus = status;
                    //connectorStatus.FCurrentStatusTime = ((statusTime.HasValue) ? statusTime.Value : DateTimeOffset.Now).DateTime;
                    connectorStatus.FCurrentStatusTime = time;

                }

                if (chargeRateKW.HasValue)
                {
                    connectorStatus.FCurrentChargeKw = Convert.ToDecimal(chargeRateKW.Value);
                }

                if (SoC.HasValue)
                {
                    connectorStatus.FStateOfCharge = Convert.ToDecimal(SoC.Value);
                }

                if (meter.HasValue)
                {
                    connectorStatus.FCurrentMeter = Convert.ToDecimal(meter.Value);
                    connectorStatus.FCurrentMeterTime = meterTime.HasValue ? meterTime.Value.UtcDateTime : DateTime.UtcNow;

                    //connectorStatus.FCurrentMeterTime = ((meterTime.HasValue) ? meterTime.Value : DateTimeOffset.Now).DateTime;
                }
                DbContext.SaveChanges();
                Logger.LogInformation("UpdateConnectorStatus => Save ConnectorStatus: ID={0} / Connector={1} / Status={2} / Meter={3}", connectorStatus.FChargerId, connectorId, status, meter);

                return true;
            }
            catch (Exception exp)
            {
                Logger.LogError(exp, "UpdateConnectorStatus => Exception writing connector status (ID={0} / Connector={1}): {2}", ChargePointStatus?.Id, connectorId, exp.Message);
            }

            return false;
        }

        protected ConnectorStatusEnum GetConnectorStatus(int connectorId)
        {
            ConnectorStatusEnum status = ConnectorStatusEnum.Undefined;
            try
            {
                var charger = DbContext.TblChargers.Where(x => x.FShortName == ChargePointStatus.Id).FirstOrDefault() ?? new TblCharger();
                var connectorStatus = DbContext.TblConnectorStatuses.Where(x => x.FChargerId == charger.FId && x.FConnectorId == connectorId).FirstOrDefault();
                if (connectorStatus != null)
                {

                    switch (connectorStatus.FCurrentStatus)
                    {
                        case "Available":
                            status = ConnectorStatusEnum.Available;
                            break;
                        case "Occupied":
                            status = ConnectorStatusEnum.Occupied;
                            break;
                        case "Faulted":
                            status = ConnectorStatusEnum.Faulted;
                            break;
                        case "Preparing":
                            status = ConnectorStatusEnum.Preparing;
                            break;
                        case "Charging":
                            status = ConnectorStatusEnum.Charging;
                            break;
                        case "Finishing":
                            status = ConnectorStatusEnum.Finishing;
                            break;
                        case "Reserved":
                            status = ConnectorStatusEnum.Reserved;
                            break;
                        case "Unavailable":
                            status = ConnectorStatusEnum.Unavailable;
                            break;
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.LogError(exp, "GetConnectorStatus => Exception writing connector status (ID={0} / Connector={1}): {2}", ChargePointStatus?.Id, connectorId, exp.Message);
            }

            return status;
        }

        /// <summary>
        /// Clean charge tag Id from possible suffix ("..._abc")
        /// </summary>
        protected static string CleanChargeTagId(string rawChargeTagId, ILogger logger)
        {
            string idTag = rawChargeTagId;

            // KEBA adds the serial to the idTag ("<idTag>_<serial>") => cut off suffix
            if (!string.IsNullOrWhiteSpace(rawChargeTagId))
            {
                int sep = rawChargeTagId.IndexOf('_');
                if (sep >= 0)
                {
                    idTag = rawChargeTagId.Substring(0, sep);
                    logger.LogTrace("CleanChargeTagId => Charge tag '{0}' => '{1}'", rawChargeTagId, idTag);
                }
            }

            return idTag;
        }

        /// <summary>
        /// Return Now + 1 year
        /// </summary>
        protected static DateTimeOffset MaxExpiryDate
        {
            get
            {
                return DateTime.UtcNow.Date.AddYears(1);
            }
        }
    }
}
