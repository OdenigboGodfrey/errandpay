# Transfer Platform Assessment

A p2p money transfer platform built with .NET, PostgreSQL (Aurora compatible), Redis etc.

The system supports account creation, account lookup, transfers, ledger tracking, and concurrency protection to prevent double-spending in a distributed environment.

---

## Architecture

The solution is separated into three projects:


TransferPlatform.Api
   - REST API endpoints
   - Business services
   - Dependency injection
   - Redis distributed locking
  
TransferPlatform.Data
   - Entity Framework Core models
   - Database context
   - Database entities
   - Data access
  
TransferPlatform.Tests
    - Transfer scenarios
    - Concurrency tests


---

## Core Features

### Account Management

Supports:

- Create account
- Retrieve account details
- Retrieve all accounts

Each account maintains:

- Unique account identifier
- Account number
- Current balance


---

## Transfers

A transfer performs:

1. Validate sender and receiver
2. Acquire distributed lock
3. Start database transaction
4. Lock account rows
5. Validate available balance
6. Debit sender
7. Credit receiver
8. Create ledger record
9. Commit transaction
10. Release distributed lock

---

## Preventing Double Spend

The application runs across multiple ECS containers, therefore a normal in-memory lock is not sufficient.

Redis is used as a distributed lock provider.

Example:

Two requests arrive at the same time:

Transfer 1:
NGN 100 → John

Transfer 2:
NGN 100 → Paul


Without locking:

Container 1 reads balance: NGN 100  
Container 2 reads balance: NGN 100  

Both transfers succeed.


With Redis locking:

Container 1:
- Acquires lock
- Completes transfer
- Releases lock


Container 2:
- Cannot acquire lock
- Transfer rejected


PostgreSQL row locking (`SELECT FOR UPDATE`) provides an additional database-level protection layer.


---

## Ledger Design

The ledger is stored as immutable transaction records.

A ledger entry contains:

- Sender account
- Receiver account
- Amount
- Currency
- Request ID
- Timestamp


The ledger acts as the audit history of all money movement.


For scalability:

- Account balance is stored for fast reads.
- Ledger entries provide the source of transaction history.
- Ledger tables can be partitioned as transaction volume grows.

---

## Data Integrity

Transfers use PostgreSQL transactions.

A transfer either:

- Completes fully
- Rolls back completely

---

## Idempotency

Transfers include a RequestId.

This prevents duplicate processing when:

- A client retries a request
- A network timeout occurs
- A payment request is submitted twice


---

## Testing

Testing in this assessment project is basic. It currently only validates:

- Successful transfers
- Concurrent transfer scenarios

However the test cases can be expanded on.

---

## Database

The application uses:
- PostgreSQL/Amazon Aurora PostgreSQL


---

## Security Considerations

Production deployment follows PCI-DSS principles:

- Aurora deployed in private subnets
- No public database endpoint
- Security groups restrict database access
- Secrets stored in AWS Secrets Manager
- IAM roles follow least privilege
- Database connections use encryption
- Pipeline execution is audited


---

## Local Development

Start dependencies:

docker compose up -d


Run API:

dotnet run --project src/TransferPlatform.Api


Run tests:

dotnet test


---

## Future Improvements

Potential production enhancements:

- Ledger partitioning
- Automated migrations/rollback
- Enhanced audit logging


---

## Summary

I am demonstrating a distributed payment service design using:

- ASP.NET Core
- Entity Framework Core
- PostgreSQL/Aurora
- Redis distributed locks
- Database transactions
- Immutable ledger records
- Secure CI/CD database migration practices

The design focuses on correctness, auditability, and safe operation in a distributed cloud environment.