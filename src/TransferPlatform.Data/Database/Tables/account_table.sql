CREATE TABLE "Accounts"
(
    "Id" uuid NOT NULL,
    "OwnerName" text NOT NULL,
    "Number" varchar(10) NOT NULL,
    "Balance" numeric(18,2) NOT NULL,
    "CreatedUtc" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Accounts_Balance_NonNegative" CHECK ("Balance" >= 0)
);

-- Unique index on Number
CREATE UNIQUE INDEX "IX_Account_Number"
    ON "Accounts" ("Number");

-- query by creation time
CREATE INDEX "IX_Accounts_CreatedUtc"
    ON "Accounts" ("CreatedUtc");
