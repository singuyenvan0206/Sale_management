import mysql from 'mysql2/promise';

// Helper to parse DATABASE_URL
const parseDatabaseUrl = (url: string) => {
  try {
    const regex = /mysql:\/\/([^:]+):([^@]+)@([^:]+):(\d+)\/(.+)/;
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
    // Fallback to defaults or environment variables if parsing fails
    return {
      host: 'localhost',
      user: 'root',
      password: '',
      database: 'main',
      port: 3306
    };
  }
};

const dbConfig = parseDatabaseUrl(process.env.DATABASE_URL || "");

// Aggressive singleton for Next.js HMR
const poolConfig: mysql.PoolOptions = {
  ...dbConfig,
  waitForConnections: true,
  connectionLimit: 5, // Keep it very low to avoid server-side exhaustion
  maxIdle: 0, // Close idle connections immediately
  idleTimeout: 10000, // 10 seconds
  queueLimit: 0,
  enableKeepAlive: true,
  keepAliveInitialDelay: 0,
};

declare global {
  var mysqlPool: mysql.Pool | undefined;
}

let pool: mysql.Pool;

if (process.env.NODE_ENV === 'production') {
  pool = mysql.createPool(poolConfig);
} else {
  if (!global.mysqlPool) {
    console.log("🛠️ Creating NEW MySQL Connection Pool...");
    global.mysqlPool = mysql.createPool(poolConfig);
  } else {
    console.log("♻️ Reusing EXISTING MySQL Connection Pool.");
  }
  pool = global.mysqlPool;
}

// Helper for row-based results
export async function query<T = any>(sql: string, params?: any[]): Promise<T[]> {
  try {
    const [rows] = await pool.execute(sql, params);
    return rows as T[];
  } catch (error) {
    console.error(`Database Query Error: ${sql}`, error);
    // If we get "Too many connections", try to log the process list
    if (error instanceof Error && error.message.includes("connections")) {
      console.warn("⚠️ DATABASE OVERLOAD DETECTED! Run 'SHOW PROCESSLIST' in your DB tool.");
    }
    throw error;
  }
}

// Helper for single object result
export async function querySingle<T = any>(sql: string, params?: any[]): Promise<T | null> {
  const rows = await query<T>(sql, params);
  return rows.length > 0 ? rows[0] : null;
}

export default pool;
