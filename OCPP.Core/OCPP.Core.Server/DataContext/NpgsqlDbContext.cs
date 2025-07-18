using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OCPP.Core.Server.Entities;

namespace OCPP.Core.Server;

public partial class NpgsqlDbContext : DbContext
{
    IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
    public NpgsqlDbContext()
    {
    }

    public NpgsqlDbContext(DbContextOptions<NpgsqlDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ConnectorStatusView> ConnectorStatusViews { get; set; }

    public virtual DbSet<TblCharger> TblChargers { get; set; }

    public virtual DbSet<TblChargerPriceShow> TblChargerPriceShows { get; set; }

    public virtual DbSet<TblChargerUnit> TblChargerUnits { get; set; }

    public virtual DbSet<TblChargingTag> TblChargingTags { get; set; }

    public virtual DbSet<TblCompany> TblCompanies { get; set; }

    public virtual DbSet<TblConnectorStatus> TblConnectorStatuses { get; set; }

    public virtual DbSet<TblEventLog> TblEventLogs { get; set; }

    public virtual DbSet<TblEventType> TblEventTypes { get; set; }

    public virtual DbSet<TblHoliday> TblHolidays { get; set; }

    public virtual DbSet<TblLanguage> TblLanguages { get; set; }

    public virtual DbSet<TblLanguageDatum> TblLanguageData { get; set; }

    public virtual DbSet<TblMessageLog> TblMessageLogs { get; set; }

    public virtual DbSet<TblPayment> TblPayments { get; set; }

    public virtual DbSet<TblPaymentApi> TblPaymentApis { get; set; }

    public virtual DbSet<TblPaymentMethod> TblPaymentMethods { get; set; }

    public virtual DbSet<TblStation> TblStations { get; set; }

    public virtual DbSet<TblTransaction> TblTransactions { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserGroup> TblUserGroups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql(config.GetConnectionString("NpgServer"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConnectorStatusView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ConnectorStatusView", "info");

            entity.Property(e => e.FCardId).HasColumnName("f_card_id");
            entity.Property(e => e.FChargerId).HasColumnName("f_charger_id");
            entity.Property(e => e.FChargerName)
                .HasMaxLength(50)
                .HasColumnName("f_charger_name");
            entity.Property(e => e.FCode)
                .HasMaxLength(10)
                .HasColumnName("f_code");
            entity.Property(e => e.FConnectorId).HasColumnName("f_connector_id");
            entity.Property(e => e.FCurrentChargeKw)
                .HasPrecision(10, 4)
                .HasColumnName("f_current_charge_kw");
            entity.Property(e => e.FCurrentMeter)
                .HasPrecision(10, 4)
                .HasColumnName("f_current_meter");
            entity.Property(e => e.FCurrentMeterTime).HasColumnName("f_current_meter_time");
            entity.Property(e => e.FCurrentStatus)
                .HasMaxLength(50)
                .HasColumnName("f_current_status");
            entity.Property(e => e.FCurrentStatusTime).HasColumnName("f_current_status_time");
            entity.Property(e => e.FEndResult)
                .HasMaxLength(20)
                .HasColumnName("f_end_result");
            entity.Property(e => e.FEndTime).HasColumnName("f_end_time");
            entity.Property(e => e.FId).HasColumnName("f_id");
            entity.Property(e => e.FMeterEnd)
                .HasPrecision(10, 4)
                .HasColumnName("f_meter_end");
            entity.Property(e => e.FMeterStart)
                .HasPrecision(10, 4)
                .HasColumnName("f_meter_start");
            entity.Property(e => e.FName)
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FShortName)
                .HasMaxLength(100)
                .HasColumnName("f_short_name");
            entity.Property(e => e.FStartResult)
                .HasMaxLength(20)
                .HasColumnName("f_start_result");
            entity.Property(e => e.FStartTime).HasColumnName("f_start_time");
            entity.Property(e => e.FStateOfCharge)
                .HasPrecision(10, 4)
                .HasColumnName("f_state_of_charge");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FTransactionNo).HasColumnName("f_transaction_no");
        });

        modelBuilder.Entity<TblCharger>(entity =>
        {
            entity.HasKey(e => new { e.FId, e.FCode }).HasName("tbl_charger_pkey");

            entity.ToTable("tbl_charger", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCode)
                .HasMaxLength(10)
                .HasColumnName("f_code");
            entity.Property(e => e.FBrand)
                .HasMaxLength(100)
                .HasColumnName("f_brand");
            entity.Property(e => e.FClientCertThumb)
                .HasMaxLength(100)
                .HasColumnName("f_client_cert_thumb");
            entity.Property(e => e.FComment)
                .HasMaxLength(100)
                .HasColumnName("f_comment");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FCurrentStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'A'::bpchar")
                .HasColumnName("f_current_status");
            entity.Property(e => e.FImage).HasColumnName("f_image");
            entity.Property(e => e.FModel)
                .HasMaxLength(100)
                .HasColumnName("f_model");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FPassword)
                .HasMaxLength(50)
                .HasColumnName("f_password");
            entity.Property(e => e.FShortName)
                .HasMaxLength(100)
                .HasColumnName("f_short_name");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FSupport)
                .HasMaxLength(50)
                .HasColumnName("f_support");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FUsername)
                .HasMaxLength(50)
                .HasColumnName("f_username");
        });

        modelBuilder.Entity<TblChargerPriceShow>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_charger_price_show_pkey");

            entity.ToTable("tbl_charger_price_show", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FChargerUnitId).HasColumnName("f_charger_unit_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FText)
                .HasMaxLength(10)
                .HasColumnName("f_text");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FValue)
                .HasPrecision(10, 2)
                .HasColumnName("f_value");
        });

        modelBuilder.Entity<TblChargerUnit>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_charger_unit_pkey");

            entity.ToTable("tbl_charger_unit", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCode)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("f_code");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
        });

        modelBuilder.Entity<TblChargingTag>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_charging_tag_pkey");

            entity.ToTable("tbl_charging_tag", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FAgencyId)
                .HasMaxLength(100)
                .HasColumnName("f_agency_id");
            entity.Property(e => e.FAuthorize)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::bpchar")
                .HasColumnName("f_authorize");
            entity.Property(e => e.FBlocked)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::bpchar")
                .HasColumnName("f_blocked");
            entity.Property(e => e.FChargerId).HasColumnName("f_charger_id");
            entity.Property(e => e.FCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("f_code");
            entity.Property(e => e.FConnectorId).HasColumnName("f_connector_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FExpiryDate).HasColumnName("f_expiry_date");
            entity.Property(e => e.FModel)
                .HasMaxLength(100)
                .HasColumnName("f_model");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FPlateNo)
                .HasMaxLength(100)
                .HasColumnName("f_plate_no");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FUserId).HasColumnName("f_user_id");
        });

        modelBuilder.Entity<TblCompany>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_company_pkey");

            entity.ToTable("tbl_company", "info", tb => tb.HasComment("สำนักงาน"));

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FAddress)
                .HasMaxLength(400)
                .HasColumnName("f_address");
            entity.Property(e => e.FCity)
                .HasMaxLength(100)
                .HasColumnName("f_city");
            entity.Property(e => e.FCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("f_code");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FEmail)
                .HasMaxLength(50)
                .HasColumnName("f_email");
            entity.Property(e => e.FFax)
                .HasMaxLength(50)
                .HasColumnName("f_fax");
            entity.Property(e => e.FInvoicenoprefix)
                .HasMaxLength(10)
                .HasColumnName("f_invoicenoprefix");
            entity.Property(e => e.FIsvat)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_isvat");
            entity.Property(e => e.FLogo)
                .HasMaxLength(100)
                .HasColumnName("f_logo");
            entity.Property(e => e.FMobile)
                .HasMaxLength(100)
                .HasColumnName("f_mobile");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("f_name");
            entity.Property(e => e.FOffice)
                .HasMaxLength(100)
                .HasColumnName("f_office");
            entity.Property(e => e.FPhone)
                .HasMaxLength(50)
                .HasColumnName("f_phone");
            entity.Property(e => e.FPostcode)
                .HasMaxLength(10)
                .HasColumnName("f_postcode");
            entity.Property(e => e.FProvince)
                .HasMaxLength(100)
                .HasColumnName("f_province");
            entity.Property(e => e.FQuotenoprefix)
                .HasMaxLength(10)
                .HasColumnName("f_quotenoprefix");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FTermsandcondition)
                .HasMaxLength(1000)
                .HasColumnName("f_termsandcondition");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FVatnumber)
                .HasMaxLength(20)
                .HasColumnName("f_vatnumber");
            entity.Property(e => e.FWebsite)
                .HasMaxLength(50)
                .HasColumnName("f_website");
        });

        modelBuilder.Entity<TblConnectorStatus>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_connector_status_pkey");

            entity.ToTable("tbl_connector_status", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FChargerId).HasColumnName("f_charger_id");
            entity.Property(e => e.FCode)
                .HasMaxLength(10)
                .HasColumnName("f_code");
            entity.Property(e => e.FConnectorId).HasColumnName("f_connector_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FCurrentChargeKw)
                .HasPrecision(10, 4)
                .HasColumnName("f_current_charge_kw");
            entity.Property(e => e.FCurrentMeter)
                .HasPrecision(10, 4)
                .HasColumnName("f_current_meter");
            entity.Property(e => e.FCurrentMeterTime)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_current_meter_time");
            entity.Property(e => e.FCurrentStatus)
                .HasMaxLength(50)
                .HasColumnName("f_current_status");
            entity.Property(e => e.FCurrentStatusTime)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_current_status_time");
            entity.Property(e => e.FName)
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FStateOfCharge)
                .HasPrecision(10, 4)
                .HasColumnName("f_state_of_charge");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FTransactionId).HasColumnName("f_transaction_id");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
        });

        modelBuilder.Entity<TblEventLog>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_event_log_pkey");

            entity.ToTable("tbl_event_log", "log");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FDescription)
                .HasMaxLength(100)
                .HasColumnName("f_description");
            entity.Property(e => e.FEventTypeId).HasColumnName("f_event_type_id");
            entity.Property(e => e.FUserId).HasColumnName("f_user_id");
        });

        modelBuilder.Entity<TblEventType>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_event_type_pkey");

            entity.ToTable("tbl_event_type", "log");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FDescription)
                .HasMaxLength(100)
                .HasColumnName("f_description");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
        });

        modelBuilder.Entity<TblHoliday>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_holiday_pkey");

            entity.ToTable("tbl_holiday", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FDay).HasColumnName("f_day");
            entity.Property(e => e.FDescription)
                .HasMaxLength(100)
                .HasColumnName("f_description");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_updated");
        });

        modelBuilder.Entity<TblLanguage>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_language_pkey");

            entity.ToTable("tbl_language", "info", tb => tb.HasComment("ภาษา"));

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCode)
                .HasMaxLength(2)
                .HasColumnName("f_code");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("f_name");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
        });

        modelBuilder.Entity<TblLanguageDatum>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_language_data_pkey");

            entity.ToTable("tbl_language_data", "info", tb => tb.HasComment("แปลภาษา"));

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FLanguageId).HasColumnName("f_language_id");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("f_name");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FText)
                .IsRequired()
                .HasMaxLength(400)
                .HasColumnName("f_text");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FValue)
                .IsRequired()
                .HasMaxLength(400)
                .HasColumnName("f_value");
        });

        modelBuilder.Entity<TblMessageLog>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_message_log_pkey");

            entity.ToTable("tbl_message_log", "log");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FChargerId).HasColumnName("f_charger_id");
            entity.Property(e => e.FConnectorId).HasColumnName("f_connector_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FDate).HasColumnName("f_date");
            entity.Property(e => e.FErrorCode)
                .HasMaxLength(100)
                .HasColumnName("f_error_code");
            entity.Property(e => e.FLogState)
                .HasMaxLength(100)
                .HasColumnName("f_log_state");
            entity.Property(e => e.FLogType)
                .HasMaxLength(100)
                .HasColumnName("f_log_type");
            entity.Property(e => e.FMessage)
                .HasMaxLength(100)
                .HasColumnName("f_message");
            entity.Property(e => e.FResult)
                .HasMaxLength(1000)
                .HasColumnName("f_result");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_payment_pkey");

            entity.ToTable("tbl_payment", "transaction");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FExpireDate).HasColumnName("f_expire_date");
            entity.Property(e => e.FNet)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("f_net");
            entity.Property(e => e.FOrderDatetime).HasColumnName("f_order_datetime");
            entity.Property(e => e.FOrderNo)
                .HasMaxLength(20)
                .HasColumnName("f_order_no");
            entity.Property(e => e.FPaymentAmount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("f_payment_amount");
            entity.Property(e => e.FPaymentCode)
                .IsRequired()
                .HasMaxLength(12)
                .HasColumnName("f_payment_code");
            entity.Property(e => e.FPaymentMethod).HasColumnName("f_payment_method");
            entity.Property(e => e.FPaymentStatus)
                .HasMaxLength(50)
                .HasColumnName("f_payment_status");
            entity.Property(e => e.FQrImage).HasColumnName("f_qr_image");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FTransactionId).HasColumnName("f_transaction_id");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FVat)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("f_vat");
        });

        modelBuilder.Entity<TblPaymentApi>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_payment_api_pkey");

            entity.ToTable("tbl_payment_api", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FMerchantId)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("f_merchant_id");
            entity.Property(e => e.FToken)
                .IsRequired()
                .HasColumnName("f_token");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FUrl)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("f_url");
        });

        modelBuilder.Entity<TblPaymentMethod>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_payment_method_pkey");

            entity.ToTable("tbl_payment_method", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
        });

        modelBuilder.Entity<TblStation>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_station_pkey");

            entity.ToTable("tbl_station", "info", tb => tb.HasComment("สถานีชาร์จ"));

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FAddress)
                .HasMaxLength(400)
                .HasColumnName("f_address");
            entity.Property(e => e.FChagerType).HasColumnName("f_chager_type");
            entity.Property(e => e.FCity)
                .HasMaxLength(100)
                .HasColumnName("f_city");
            entity.Property(e => e.FCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("f_code");
            entity.Property(e => e.FCompanyId).HasColumnName("f_company_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FEmail)
                .HasMaxLength(50)
                .HasColumnName("f_email");
            entity.Property(e => e.FFax)
                .HasMaxLength(50)
                .HasColumnName("f_fax");
            entity.Property(e => e.FImage).HasColumnName("f_image");
            entity.Property(e => e.FInvoicenoprefix)
                .HasMaxLength(10)
                .HasColumnName("f_invoicenoprefix");
            entity.Property(e => e.FIsvat)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_isvat");
            entity.Property(e => e.FLatitude).HasColumnName("f_latitude");
            entity.Property(e => e.FLogo).HasColumnName("f_logo");
            entity.Property(e => e.FLongitude).HasColumnName("f_longitude");
            entity.Property(e => e.FMinimumAmount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("30")
                .HasColumnName("f_minimum_amount");
            entity.Property(e => e.FMobile)
                .HasMaxLength(100)
                .HasColumnName("f_mobile");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("f_name");
            entity.Property(e => e.FOffice)
                .HasMaxLength(100)
                .HasColumnName("f_office");
            entity.Property(e => e.FOffpeak)
                .HasPrecision(10, 2)
                .HasColumnName("f_offpeak");
            entity.Property(e => e.FOnpeak)
                .HasPrecision(10, 2)
                .HasColumnName("f_onpeak");
            entity.Property(e => e.FPhone)
                .HasMaxLength(50)
                .HasColumnName("f_phone");
            entity.Property(e => e.FPostcode)
                .HasMaxLength(10)
                .HasColumnName("f_postcode");
            entity.Property(e => e.FProvince)
                .HasMaxLength(100)
                .HasColumnName("f_province");
            entity.Property(e => e.FQuotenoprefix)
                .HasMaxLength(10)
                .HasColumnName("f_quotenoprefix");
            entity.Property(e => e.FRfid)
                .HasMaxLength(20)
                .HasColumnName("f_rfid");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FTermsandcondition)
                .HasMaxLength(1000)
                .HasColumnName("f_termsandcondition");
            entity.Property(e => e.FUnitId).HasColumnName("f_unit_id");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FVatnumber)
                .HasMaxLength(20)
                .HasColumnName("f_vatnumber");
            entity.Property(e => e.FWebsite)
                .HasMaxLength(50)
                .HasColumnName("f_website");
        });

        modelBuilder.Entity<TblTransaction>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_transaction_pkey");

            entity.ToTable("tbl_transaction", "transaction");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCardId).HasColumnName("f_card_id");
            entity.Property(e => e.FChargerId).HasColumnName("f_charger_id");
            entity.Property(e => e.FConnectorId).HasColumnName("f_connector_id");
            entity.Property(e => e.FCost)
                .HasPrecision(10, 2)
                .HasColumnName("f_cost");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FEndResult)
                .HasMaxLength(20)
                .HasColumnName("f_end_result");
            entity.Property(e => e.FEndTime).HasColumnName("f_end_time");
            entity.Property(e => e.FMeterEnd)
                .HasPrecision(10, 4)
                .HasColumnName("f_meter_end");
            entity.Property(e => e.FMeterStart)
                .HasPrecision(10, 4)
                .HasColumnName("f_meter_start");
            entity.Property(e => e.FPreMeter).HasColumnName("f_pre_meter");
            entity.Property(e => e.FStartResult)
                .HasMaxLength(20)
                .HasColumnName("f_start_result");
            entity.Property(e => e.FStartTime).HasColumnName("f_start_time");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FTransactionNo).HasColumnName("f_transaction_no");
            entity.Property(e => e.FTransactionStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValueSql("'Waiting'::character varying")
                .HasColumnName("f_transaction_status");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FUserId).HasColumnName("f_user_id");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_user_pkey");

            entity.ToTable("tbl_user", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FAddress)
                .HasMaxLength(100)
                .HasColumnName("f_address");
            entity.Property(e => e.FCity)
                .HasMaxLength(50)
                .HasColumnName("f_city");
            entity.Property(e => e.FCompanyId).HasColumnName("f_company_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FEmail)
                .HasMaxLength(50)
                .HasColumnName("f_email");
            entity.Property(e => e.FImage)
                .HasMaxLength(100)
                .HasColumnName("f_image");
            entity.Property(e => e.FLanguage)
                .IsRequired()
                .HasMaxLength(2)
                .HasDefaultValueSql("'TH'::character varying")
                .HasColumnName("f_language");
            entity.Property(e => e.FLastlogin).HasColumnName("f_lastlogin");
            entity.Property(e => e.FLastname)
                .HasMaxLength(100)
                .HasColumnName("f_lastname");
            entity.Property(e => e.FMobile)
                .HasMaxLength(50)
                .HasColumnName("f_mobile");
            entity.Property(e => e.FName)
                .HasMaxLength(100)
                .HasColumnName("f_name");
            entity.Property(e => e.FPassword)
                .HasMaxLength(256)
                .HasColumnName("f_password");
            entity.Property(e => e.FPostcode)
                .HasMaxLength(10)
                .HasColumnName("f_postcode");
            entity.Property(e => e.FProvince)
                .HasMaxLength(50)
                .HasColumnName("f_province");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FToken)
                .HasMaxLength(400)
                .HasColumnName("f_token");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
            entity.Property(e => e.FUserGroupId).HasColumnName("f_user_group_id");
            entity.Property(e => e.FUsername)
                .HasMaxLength(100)
                .HasColumnName("f_username");
        });

        modelBuilder.Entity<TblUserGroup>(entity =>
        {
            entity.HasKey(e => e.FId).HasName("tbl_user_group_pkey");

            entity.ToTable("tbl_user_group", "info");

            entity.Property(e => e.FId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("f_id");
            entity.Property(e => e.FCreateby).HasColumnName("f_createby");
            entity.Property(e => e.FCreated)
                .HasDefaultValueSql("now()")
                .HasColumnName("f_created");
            entity.Property(e => e.FIssystem)
                .HasMaxLength(1)
                .HasDefaultValueSql("'N'::bpchar")
                .HasColumnName("f_issystem");
            entity.Property(e => e.FName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("f_name");
            entity.Property(e => e.FStationId).HasColumnName("f_station_id");
            entity.Property(e => e.FStatus)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("f_status");
            entity.Property(e => e.FUpdateby).HasColumnName("f_updateby");
            entity.Property(e => e.FUpdated).HasColumnName("f_updated");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
