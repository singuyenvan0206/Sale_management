import NextAuth from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";
import { querySingle } from "@/lib/db";
import { verifyPassword } from "@/lib/auth-utils";

const handler = NextAuth({
  providers: [
    CredentialsProvider({
      name: "Credentials",
      credentials: {
        username: { label: "Username", type: "text", placeholder: "admin" },
        password: { label: "Password", type: "password" }
      },
      async authorize(credentials) {
        if (!credentials?.username || !credentials?.password) return null;

        // Using mysql2 query instead of Prisma
        const user = await querySingle(
          "SELECT * FROM accounts WHERE Username = ?", 
          [credentials.username]
        );

        if (user && verifyPassword(credentials.password, user.Password)) {
          return {
            id: user.Id.toString(),
            name: user.Username,
            role: user.Role,
          };
        }
        return null;
      }
    })
  ],
  callbacks: {
    async jwt({ token, user }) {
      if (user) {
        token.id = user.id;
        token.role = (user as any).role;
      }
      return token;
    },
    async session({ session, token }) {
      if (token) {
        (session.user as any).id = token.id;
        (session.user as any).role = token.role;
      }
      return session;
    }
  },
  pages: {
    signIn: "/login",
  },
  session: {
    strategy: "jwt",
  },
});

export { handler as GET, handler as POST };
