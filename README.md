# ChargeMe Solution

## Overview
This solution contains multiple projects for the ChargeMe electric vehicle charging system.

## Projects

### Backend Services
- **chargeme-app.Server** - Main API server for the charging application
- **chargeme-api** - Additional API services
- **csms** - Charge Station Management System
- **manager** - Management interface backend
- **chargeme-app.admin** - Admin panel backend

### Frontend Applications
- **chargeme-app.client** - Vue.js client application
- **chargeme-app** - Static web application
- **chargeme-app.csms** - CSMS frontend application

### Core Libraries
- **OCPP.Core** - Open Charge Point Protocol core library
- **OCPP.Core.Database** - Database entities and context
- **OCPP.Core.Lib** - Core library components
- **OCPP.Core.Management** - Management components
- **OCPP.Core.Server** - OCPP server implementation
- **OCPP.Core.NewDatabase** - New database schema
- **OCPP.MyCore.Server** - Custom OCPP server

## Architecture
The solution follows a microservices architecture with separate projects for different functionalities:
- Authentication and user management
- Charging station operations
- Payment processing
- Transaction management
- Real-time communication via SignalR hubs

## Getting Started
1. Ensure you have .NET 6+ installed
2. Configure your database connection strings in `appsettings.json`
3. Run the database migrations
4. Start the required services based on your needs

## Notes
- The TransactionController handles payment processing and transaction history
- All API endpoints require authentication via JWT tokens
- The system supports multiple payment gateways
- Real-time updates are handled through SignalR hubs 