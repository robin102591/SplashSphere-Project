/**
 * Tenant-level admin settings DTOs.
 * Mirrors `SplashSphere.Application.Features.Settings.*` on the backend.
 */

import type { LogoPosition, LogoSize, ReceiptFontSize, ReceiptWidth } from './enums';

// ── Company profile ───────────────────────────────────────────────────────────

export interface CompanyProfileDto {
  // Identity
  readonly name: string;
  readonly tagline: string | null;

  // Contact
  readonly email: string;
  readonly contactNumber: string;
  readonly website: string | null;

  // Address (legacy single-string + structured)
  readonly address: string;
  readonly streetAddress: string | null;
  readonly barangay: string | null;
  readonly city: string | null;
  readonly province: string | null;
  readonly zipCode: string | null;

  // Tax & registration
  readonly taxId: string | null;
  readonly businessPermitNo: string | null;
  readonly isVatRegistered: boolean;

  // Social & payment
  readonly facebookUrl: string | null;
  readonly instagramHandle: string | null;
  readonly gcashNumber: string | null;
}

/**
 * PUT body for `/api/v1/settings/company`. Same shape as
 * `CompanyProfileDto` minus the derived single-string `address` (the
 * server re-derives it from the structured fields on save).
 */
export interface UpdateCompanyProfilePayload {
  name: string;
  tagline: string | null;
  email: string;
  contactNumber: string;
  website: string | null;
  streetAddress: string | null;
  barangay: string | null;
  city: string | null;
  province: string | null;
  zipCode: string | null;
  taxId: string | null;
  businessPermitNo: string | null;
  isVatRegistered: boolean;
  facebookUrl: string | null;
  instagramHandle: string | null;
  gcashNumber: string | null;
}

// ── Receipt designer ──────────────────────────────────────────────────────────

/**
 * Receipt-design configuration. `branchId === null` is the tenant-default
 * row; non-null values are per-branch overrides (slice 4).
 */
export interface ReceiptSettingDto {
  readonly branchId: string | null;

  // Header
  readonly showLogo: boolean;
  readonly logoSize: LogoSize;
  readonly logoPosition: LogoPosition;
  readonly showBusinessName: boolean;
  readonly showTagline: boolean;
  readonly showBranchName: boolean;
  readonly showBranchAddress: boolean;
  readonly showBranchContact: boolean;
  readonly showTIN: boolean;
  readonly customHeaderText: string | null;

  // Body
  readonly showServiceDuration: boolean;
  readonly showEmployeeNames: boolean;
  readonly showVehicleInfo: boolean;
  readonly showDiscountBreakdown: boolean;
  readonly showTaxLine: boolean;
  readonly showTransactionNumber: boolean;
  readonly showDateTime: boolean;
  readonly showCashierName: boolean;

  // Customer
  readonly showCustomerName: boolean;
  readonly showCustomerPhone: boolean;
  readonly showLoyaltyPointsEarned: boolean;
  readonly showLoyaltyBalance: boolean;
  readonly showLoyaltyTier: boolean;

  // Footer
  readonly thankYouMessage: string;
  readonly promoText: string | null;
  readonly showSocialMedia: boolean;
  readonly showGCashQr: boolean;
  readonly showGCashNumber: boolean;
  readonly customFooterText: string | null;

  // Format
  readonly receiptWidth: ReceiptWidth;
  readonly fontSize: ReceiptFontSize;
  readonly autoCutPaper: boolean;
}

/** PUT body for `/api/v1/settings/receipt`. Branch ID lives in the query string. */
export interface UpdateReceiptSettingPayload {
  // Header
  showLogo: boolean;
  logoSize: LogoSize;
  logoPosition: LogoPosition;
  showBusinessName: boolean;
  showTagline: boolean;
  showBranchName: boolean;
  showBranchAddress: boolean;
  showBranchContact: boolean;
  showTIN: boolean;
  customHeaderText: string | null;

  // Body
  showServiceDuration: boolean;
  showEmployeeNames: boolean;
  showVehicleInfo: boolean;
  showDiscountBreakdown: boolean;
  showTaxLine: boolean;
  showTransactionNumber: boolean;
  showDateTime: boolean;
  showCashierName: boolean;

  // Customer
  showCustomerName: boolean;
  showCustomerPhone: boolean;
  showLoyaltyPointsEarned: boolean;
  showLoyaltyBalance: boolean;
  showLoyaltyTier: boolean;

  // Footer
  thankYouMessage: string;
  promoText: string | null;
  showSocialMedia: boolean;
  showGCashQr: boolean;
  showGCashNumber: boolean;
  customFooterText: string | null;

  // Format
  receiptWidth: ReceiptWidth;
  fontSize: ReceiptFontSize;
  autoCutPaper: boolean;
}
