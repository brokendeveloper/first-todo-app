CREATE TABLE "AuditLogs" (
                             "Id" SERIAL PRIMARY KEY,
                             "UserId" INTEGER,
                             "ActionId" INTEGER NOT NULL,
                             "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
                             "Description" JSONB
);

CREATE INDEX "IX_AuditLogs_Description" ON "AuditLogs" USING GIN ("Description");
