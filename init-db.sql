# Create database schema
CREATE TABLE IF NOT EXISTS "Organizers" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" character varying(255) NOT NULL,
    "Email" character varying(255) NOT NULL UNIQUE,
    "Organization" character varying(255),
    "IsVerified" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Campaigns" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "Title" character varying(255) NOT NULL,
    "Description" text,
    "GoalAmount" numeric(18,2) NOT NULL,
    "CurrentAmount" numeric(18,2) NOT NULL DEFAULT 0,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone,
    "Status" integer NOT NULL DEFAULT 0,
    "OrganizerId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("OrganizerId") REFERENCES "Organizers"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Donations" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "CampaignId" uuid NOT NULL,
    "DonorName" character varying(255) NOT NULL,
    "DonorEmail" character varying(255),
    "Amount" numeric(18,2) NOT NULL,
    "Message" text,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsAnonymous" boolean NOT NULL DEFAULT false,
    FOREIGN KEY ("CampaignId") REFERENCES "Campaigns"("Id") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Campaigns_OrganizerId" ON "Campaigns"("OrganizerId");
CREATE INDEX IF NOT EXISTS "IX_Campaigns_Status" ON "Campaigns"("Status");
CREATE INDEX IF NOT EXISTS "IX_Donations_CampaignId" ON "Donations"("CampaignId");
CREATE INDEX IF NOT EXISTS "IX_Donations_CreatedAt" ON "Donations"("CreatedAt");
