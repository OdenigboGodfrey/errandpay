CREATE TABLE "LedgerEntries"
(
    "Id" uuid NOT NULL,
    "FromAccountId" uuid NOT NULL,
    "ToAccountId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "Currency" varchar(5) NOT NULL,
    "RequestId" varchar(100) NOT NULL,
    "CreatedUtc" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_LedgerEntries"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_LedgerEntries_Accounts_FromAccountId"
        FOREIGN KEY ("FromAccountId")
        REFERENCES "Accounts" ("Id")
        ON DELETE RESTRICT,

    CONSTRAINT "FK_LedgerEntries_Accounts_ToAccountId"
        FOREIGN KEY ("ToAccountId")
        REFERENCES "Accounts" ("Id")
        ON DELETE RESTRICT,
    
    CONSTRAINT "CK_LedgerEntries_Amount_Positive" CHECK ("Amount" > 0),

    CONSTRAINT "CK_LedgerEntries_DifferentAccounts" CHECK ("FromAccountId" <> "ToAccountId")
);

-- Unique RequestId
CREATE UNIQUE INDEX "IX_LedgerEntries_RequestId"
    ON "LedgerEntries" ("RequestId");

-- query by sender
CREATE INDEX "IX_LedgerEntries_FromAccountId"
    ON "LedgerEntries" ("FromAccountId");

-- query by receiver
CREATE INDEX "IX_LedgerEntries_ToAccountId"
    ON "LedgerEntries" ("ToAccountId");

-- query by creation time
CREATE INDEX "IX_LedgerEntries_CreatedUtc"
    ON "LedgerEntries" ("CreatedUtc");