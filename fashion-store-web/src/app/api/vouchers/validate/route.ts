import { querySingle } from "@/lib/db";
import { Success, ErrorResponse, ServerError } from "@/lib/api-response";

export async function GET(request: Request) {
  const { searchParams } = new URL(request.url);
  const code = searchParams.get("code");

  if (!code) {
    return ErrorResponse("Missing voucher code", 400);
  }

  try {
    const voucher: any = await querySingle(
      'SELECT * FROM "vouchers" WHERE "Code" = $1 AND "IsActive" = true',
      [code.toUpperCase()]
    );

    if (!voucher) {
      return ErrorResponse("Voucher không tồn tại hoặc đã hết hạn", 404);
    }

    // Check dates and usage limit
    const now = new Date();
    if (voucher.StartDate && new Date(voucher.StartDate) > now) {
      return ErrorResponse("Voucher chưa đến ngày sử dụng", 400);
    }
    if (voucher.EndDate && new Date(voucher.EndDate) < now) {
      return ErrorResponse("Voucher đã hết hạn", 400);
    }
    if (voucher.UsageLimit && voucher.UsedCount >= voucher.UsageLimit) {
      return ErrorResponse("Voucher đã hết lượt sử dụng", 400);
    }

    return Success(voucher, "Voucher validated successfully");
  } catch (error) {
    return ServerError(error);
  }
}
