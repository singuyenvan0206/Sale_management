const { Pool } = require('pg');

const pool = new Pool({
  user: 'postgres',
  password: '02062003',
  host: 'localhost',
  database: 'Sale',
  port: 5432
});

async function main() {
  try {
    console.log("Setting up settings table...");
    await pool.query(`
      CREATE TABLE IF NOT EXISTS "settings" (
        "Key" VARCHAR(50) PRIMARY KEY,
        "Value" TEXT,
        "UpdatedAt" TIMESTAMP DEFAULT NOW()
      )
    `);
    
    const defaultSettings = [
      ['BankId', 'MB'],
      ['AccountNumber', '020120248888'],
      ['AccountName', 'FASHION STORE HÀ NỘI'],
      ['StoreName', 'FASHION BOUTIQUE'],
      ['StoreAddress', '123 Đường Láng, Hà Nội'],
      ['StorePhone', '0988 888 888']
    ];

    for (const [key, value] of defaultSettings) {
      await pool.query(
        'INSERT INTO "settings" ("Key", "Value", "UpdatedAt") VALUES ($1, $2, NOW()) ON CONFLICT ("Key") DO UPDATE SET "Value" = EXCLUDED."Value", "UpdatedAt" = NOW()',
        [key, value]
      );
    }
    
    console.log("✅ Settings initialized successfully.");
  } catch (err) {
    console.error("❌ Setup failed:", err);
  } finally {
    await pool.end();
  }
}

main();
