START TRANSACTION;
ALTER TABLE applications ADD deploy_key character varying(128);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260818055832_AddApplicationFields', '10.0.11');

COMMIT;

