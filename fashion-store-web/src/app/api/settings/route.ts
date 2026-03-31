import { NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";

export async function GET() {
  try {
    const settings = await query("SELECT * FROM settings");
    // Convert array to object for easier consumption
    const settingsObj = settings.reduce((acc: any, curr: any) => {
      acc[curr.Key] = curr.Value;
      return acc;
    }, {});
    
    return NextResponse.json(settingsObj);
  } catch (error) {
    return NextResponse.json({ error: "Failed to fetch settings" }, { status: 500 });
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();
    
    // Efficiently update each setting
    for (const [key, value] of Object.entries(body)) {
      await query(
        "INSERT INTO settings (\`Key\`, \`Value\`, UpdatedAt) VALUES (?, ?, NOW()) ON DUPLICATE KEY UPDATE \`Value\` = ?, UpdatedAt = NOW()",
        [key, value, value]
      );
    }
    
    return NextResponse.json({ message: "Settings updated successfully" });
  } catch (error) {
    return NextResponse.json({ error: "Failed to update settings" }, { status: 500 });
  }
}
