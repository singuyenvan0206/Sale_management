import { Pool, PoolConfig } from 'pg';

// Helper to parse DATABASE_URL (supports postgres:// and postgresql:// formats)
const parseDatabaseUrl = (url: string) => {
  try {
    const regex = /(?:postgres|postgresql):\/\/([^:]+):([^@]+)@([^:]+):(\d+)\/(.+)/;
    const match = url.match(regex);
    if (!match) throw new Error("Invalid DATABASE_URL format");
    
    return {
      user: match[1],
      password: match[2],
      host: match[3],
      port: parseInt(match[4]),
      database: match[5]
    };
  } catch (error) {
    console.error("Error parsing DATABASE_URL:", error);
    // Fallback to local Postgres as requested
    return {
      host: 'localhost',
      user: 'postgres',
      password: '02062003',
      database: 'Sale',
      port: 5432
    };
  }
};

const dbConfig = parseDatabaseUrl(process.env.DATABASE_URL || "");

const poolConfig: PoolConfig = {
  ...dbConfig,
  max: 10,
  idleTimeoutMillis: 30000,
  connectionTimeoutMillis: 2000,
};

declare global {
  var pgPool: Pool | undefined;
}

const pool = global.pgPool || new Pool(poolConfig);
if (process.env.NODE_ENV !== 'production') global.pgPool = pool;

// Helper for row-based results
export async function query<T = any>(sql: string, params?: any[]): Promise<T[]> {
  try {
    const result = await pool.query(sql, params);
    return result.rows as T[];
  } catch (error) {
    console.error(`Database Query Error: ${sql}`, error);
    throw error;
  }
}

// Helper for single object result
export async function querySingle<T = any>(sql: string, params?: any[]): Promise<T | null> {
  const rows = await query<T>(sql, params);
  return rows.length > 0 ? rows[0] : null;
}

export default pool;
