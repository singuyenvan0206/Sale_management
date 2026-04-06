const { Pool } = require('pg');

const pool = new Pool({
  user: 'postgres',
  password: '02062003',
  host: 'localhost',
  database: 'Sale',
  port: 5432
});

async function check() {
  try {
    const res = await pool.query("SELECT table_name, column_name FROM information_schema.columns WHERE table_schema = 'public' ORDER BY table_name, ordinal_position");
    const tables = {};
    res.rows.forEach(r => {
      if (!tables[r.table_name]) tables[r.table_name] = [];
      tables[r.table_name].push(r.column_name);
    });
    console.log(JSON.stringify(tables, null, 2));
  } catch (err) {
    console.error(err);
  } finally {
    await pool.end();
  }
}

check();
