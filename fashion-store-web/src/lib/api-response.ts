import { NextResponse } from "next/server";

/**
 * Standard API Response structure for Fusion ERP
 */
export type ApiResponse<T = any> = {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
};

export const Success = <T = any>(data: T, message?: string, status = 200) => {
  return NextResponse.json({
    success: true,
    data,
    message
  }, { status });
};

export const ErrorResponse = (error: string, status = 400) => {
  return NextResponse.json({
    success: false,
    error
  }, { status });
};

export const ServerError = (error: any) => {
  console.error("API_SERVER_ERROR:", error);
  return NextResponse.json({
    success: false,
    error: "Hệ thống gặp sự cố. Vui lòng thử lại sau."
  }, { status: 500 });
};

export const Unauthorized = () => {
  return NextResponse.json({
    success: false,
    error: "Phiên làm việc hết hạn hoặc không có quyền truy cập."
  }, { status: 401 });
};
