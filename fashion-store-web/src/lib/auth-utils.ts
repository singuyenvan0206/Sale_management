import crypto from 'crypto';

/**
 * Replicates the WPF PasswordHelper.VerifyPassword logic
 * 1. Base64 decode string
 * 2. Extract salt (first 16 bytes)
 * 3. Extract stored hash (next 32 bytes)
 * 4. Combine input password + salt
 * 5. SHA256 hash
 * 6. Compare
 */
export function verifyPassword(password: string, hashedPassword?: string | null): boolean {
  if (!password || !hashedPassword) return false;

  try {
    const storedBytes = Buffer.from(hashedPassword, 'base64');
    
    // Salt is first 16 bytes
    const salt = storedBytes.subarray(0, 16);
    
    // Hash is next 32 bytes
    const storedHash = storedBytes.subarray(16, 48);

    // Combine password + salt
    const passwordBytes = Buffer.from(password, 'utf-8');
    const saltedPassword = Buffer.concat([passwordBytes, salt]);

    // Compute hash
    const computedHash = crypto.createHash('sha256').update(saltedPassword).digest();

    // Constant-time comparison
    if (storedHash.length !== computedHash.length) return false;
    return crypto.timingSafeEqual(storedHash, computedHash);
  } catch (error) {
    // Fallback to plain comparison if not hashed (for legacy or initial admin)
    return password === hashedPassword;
  }
}

/**
 * Replicates the WPF PasswordHelper.HashPassword logic
 */
export function hashPassword(password: string): string {
  // Generate 16-byte random salt
  const salt = crypto.randomBytes(16);

  // Combine password + salt
  const passwordBytes = Buffer.from(password, 'utf-8');
  const saltedPassword = Buffer.concat([passwordBytes, salt]);

  // Compute hash
  const hash = crypto.createHash('sha256').update(saltedPassword).digest();

  // Combine salt + hash
  const result = Buffer.concat([salt, hash]);

  // Base64 encode
  return result.toString('base64');
}
