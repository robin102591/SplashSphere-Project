/**
 * Tenant-level admin settings DTOs.
 * Mirrors `SplashSphere.Application.Features.Settings.*` on the backend.
 */

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
