const { Pool } = require('pg');

const pool = new Pool({
  user: 'postgres',
  password: '02062003',
  host: 'localhost',
  database: 'Sale',
  port: 5432
});

async function fix() {
  try {
    console.log("Starting schema fix...");
    
    // Create StockMovements table
    await pool.query(`
      CREATE TABLE IF NOT EXISTS "StockMovements" (
        "Id" SERIAL PRIMARY KEY,
        "ProductId" INTEGER NOT NULL,
        "Type" VARCHAR(10),
        "Quantity" INTEGER,
        "ReferenceId" INTEGER,
        "Note" TEXT,
        "CreatedDate" TIMESTAMP DEFAULT NOW()
      )
    `);
    console.log("✅ Created StockMovements table.");

    // Add MinStockLevel to products
    try {
      await pool.query('ALTER TABLE "products" ADD COLUMN "MinStockLevel" INTEGER DEFAULT 5');
      console.log("✅ Added MinStockLevel column to products.");
    } catch (e) {
      if (e.code === '42701') {
        console.log("ℹ️ MinStockLevel column already exists.");
      } else {
        throw e;
      }
    }

    console.log("🚀 Schema alignment completed successfully!");
  } catch (err) {
    console.error("❌ Schema fix failed:", err);
  } finally {
    await pool.end();
  }
}

fix();
