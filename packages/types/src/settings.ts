/**
 * Tenant-level admin settings DTOs.
 * Mirrors `SplashSphere.Application.Features.Settings.*` on the backend.
 */

import type {
  DisplayFontSize,
  DisplayOrientation,
  DisplayTheme,
  LogoPosition,
  LogoSize,
  ReceiptFontSize,
  ReceiptWidth,
} from './enums';

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

  // Logo (slice 3) — managed by POST/DELETE /settings/company/logo, NOT
  // by PUT /settings/company. The three URLs are returned with
  // cache-busting query suffixes so the browser refetches after re-upload.
  readonly logoUrl: string | null;
  readonly logoThumbnailUrl: string | null;
  readonly logoIconUrl: string | null;
}

/** Response from POST /api/v1/settings/company/logo. */
export interface UploadLogoResult {
  readonly logoUrl: string;
  readonly logoThumbnailUrl: string;
  readonly logoIconUrl: string;
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

// ── Customer display ──────────────────────────────────────────────────────────

export interface DisplaySettingDto {
  readonly branchId: string | null;

  // Idle
  readonly showLogo: boolean;
  readonly showBusinessName: boolean;
  readonly showTagline: boolean;
  readonly showDateTime: boolean;
  readonly showGCashQr: boolean;
  readonly showSocialMedia: boolean;
  readonly promoMessages: readonly string[];
  readonly promoRotationSeconds: number;

  // Building / transaction
  readonly showVehicleInfo: boolean;
  readonly showCustomerName: boolean;
  readonly showLoyaltyTier: boolean;
  readonly showDiscountBreakdown: boolean;
  readonly showTaxLine: boolean;

  // Completion
  readonly showPaymentMethod: boolean;
  readonly showChangeAmount: boolean;
  readonly showPointsEarned: boolean;
  readonly showPointsBalance: boolean;
  readonly showThankYouMessage: boolean;
  readonly showPromoText: boolean;
  readonly completionHoldSeconds: number;

  // Appearance
  readonly theme: DisplayTheme;
  readonly fontSize: DisplayFontSize;
  readonly orientation: DisplayOrientation;
}

/**
 * Mutable shape of {@link DisplaySettingDto} — drops `readonly` and `branchId`
 * (branch comes from the query string on PUT, not the payload).
 */
export interface UpdateDisplaySettingPayload {
  // Idle
  showLogo: boolean;
  showBusinessName: boolean;
  showTagline: boolean;
  showDateTime: boolean;
  showGCashQr: boolean;
  showSocialMedia: boolean;
  promoMessages: string[];
  promoRotationSeconds: number;

  // Building / transaction
  showVehicleInfo: boolean;
  showCustomerName: boolean;
  showLoyaltyTier: boolean;
  showDiscountBreakdown: boolean;
  showTaxLine: boolean;

  // Completion
  showPaymentMethod: boolean;
  showChangeAmount: boolean;
  showPointsEarned: boolean;
  showPointsBalance: boolean;
  showThankYouMessage: boolean;
  showPromoText: boolean;
  completionHoldSeconds: number;

  // Appearance
  theme: DisplayTheme;
  fontSize: DisplayFontSize;
  orientation: DisplayOrientation;
}

/**
 * Customer-display-safe subset of the tenant's company profile. Includes
 * only fields that are appropriate for a public, customer-facing screen —
 * NOT tax IDs, business permits, or other compliance metadata.
 */
export interface DisplayBrandingDto {
  readonly businessName: string;
  readonly tagline: string | null;
  readonly logoUrl: string | null;
  readonly facebookUrl: string | null;
  readonly instagramHandle: string | null;
  readonly gCashNumber: string | null;
  readonly gCashQrUrl: string | null;
}

/**
 * Combined render config for the customer-facing display device. Returned by
 * `GET /api/v1/display/config?branchId={id}` so the device fetches once at
 * boot and then listens for SignalR events.
 */
export interface DisplayConfigDto {
  readonly settings: DisplaySettingDto;
  readonly branding: DisplayBrandingDto;
}
