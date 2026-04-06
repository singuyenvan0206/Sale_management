const { Pool } = require('pg');
const pool = new Pool({
  host: 'localhost',
  user: 'postgres',
  password: '02062003',
  database: 'Sale',
  port: 5432
});

async function main() {
  try {
    const res = await pool.query("SELECT column_name FROM information_schema.columns WHERE table_name = 'invoices'");
    console.log("Columns of 'invoices':", JSON.stringify(res.rows.map(r => r.column_name), null, 2));
  } catch (e) {
    console.error(e);
  } finally {
    await pool.end();
  }
}

main();
