import { NextRequest, NextResponse } from "next/server";
import { query, querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError, Unauthorized } from "@/lib/api-response";
import { getServerSession } from "next-auth";

export async function GET(req: NextRequest) {
  const session = await getServerSession();
  if (!session) return Unauthorized();

  try {
    const { searchParams } = new URL(req.url);
    const q = searchParams.get("q") || "";

    let sql = 'SELECT "Id", "Username", "Role", "EmployeeName", "LastLoginDate", "IsActive", "CreatedDate" FROM "accounts" WHERE 1=1';
    const params: any[] = [];

    if (q) {
      sql += ` AND ("Username" ILIKE $${params.length + 1} OR "EmployeeName" ILIKE $${params.length + 2})`;
      params.push(`%${q}%`, `%${q}%`);
    }

    sql += ' ORDER BY "CreatedDate" DESC';

    const employees = await query(sql, params);
    return Success(employees);
  } catch (error) {
    return ServerError(error);
  }
}

export async function POST(request: Request) {
  const session = await getServerSession();
  if (!session || (session as any).user?.role !== 'Admin') return Unauthorized();

  try {
    const body = await request.json();
    const { Username, Password, Role, EmployeeName } = body;

    if (!Username || !Password || !EmployeeName) return ErrorResponse("Vui lòng nhập đầy đủ Tài khoản, Mật khẩu và Tên nhân viên.");

    // Check duplicate
    const existing = await querySingle('SELECT "Id" FROM "accounts" WHERE "Username" = $1', [Username]);
    if (existing) return ErrorResponse(`Tài khoản "${Username}" đã tồn tại.`);

    await query(
      'INSERT INTO "accounts" ("Username", "Password", "Role", "EmployeeName", "IsActive", "CreatedDate") VALUES ($1, $2, $3, $4, true, NOW())',
      [Username, Password, Role || 'Staff', EmployeeName]
    );

    return Success(null, "Tài khoản nhân viên đã được khởi tạo thành công.", 201);
  } catch (error) {
    return ServerError(error);
  }
}

export async function PUT(request: Request) {
  const session = await getServerSession();
  if (!session || (session as any).user?.role !== 'Admin') return Unauthorized();

  try {
    const body = await request.json();
    const { Id, Role, EmployeeName, IsActive, Password } = body;

    if (!Id) return ErrorResponse("Thiếu ID tài khoản.");

    if (Password) {
      await query(
        `UPDATE "accounts" SET "Role" = $1, "EmployeeName" = $2, "IsActive" = $3, "Password" = $4, "UpdatedDate" = NOW() WHERE "Id" = $5`,
        [Role, EmployeeName, IsActive, Password, Id]
      );
    } else {
      await query(
        `UPDATE "accounts" SET "Role" = $1, "EmployeeName" = $2, "IsActive" = $3, "UpdatedDate" = NOW() WHERE "Id" = $4`,
        [Role, EmployeeName, IsActive, Id]
      );
    }

    return Success(null, "Cập nhật tài khoản nhân viên thành công.");
  } catch (error) {
    return ServerError(error);
  }
}

export async function DELETE(request: Request) {
  const session = await getServerSession();
  if (!session || (session as any).user?.role !== 'Admin') return Unauthorized();

  try {
    const { searchParams } = new URL(request.url);
    const id = searchParams.get("id");
    if (!id) return ErrorResponse("Thiếu ID tài khoản.");

    // Check if employee has recorded any invoices/movements
    const hasHistory = await querySingle('SELECT 1 FROM "invoices" WHERE "EmployeeId" = $1 LIMIT 1', [id]);
    if (hasHistory) {
      await query('UPDATE "accounts" SET "IsActive" = false, "UpdatedDate" = NOW() WHERE "Id" = $1', [id]);
      return Success(null, "Nhân viên đã có lịch sử công tác nên đã được vô hiệu hóa tài khoản thay vì xóa.");
    }

    await query('DELETE FROM "accounts" WHERE "Id" = $1', [id]);
    return Success(null, "Đã xóa tài khoản nhân viên vĩnh viễn.");
  } catch (error) {
    return ServerError(error);
  }
}
