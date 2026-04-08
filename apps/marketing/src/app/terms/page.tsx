import type { Metadata } from 'next'
import Link from 'next/link'

export const metadata: Metadata = {
  title: 'Terms of Service',
}

export default function TermsOfServicePage() {
  return (
    <>
      {/* Hero Banner */}
      <section className="bg-splash-900 py-12 text-white">
        <div className="mx-auto max-w-3xl px-6 text-center">
          <h1 className="text-4xl font-bold tracking-tight">
            Terms of Service
          </h1>
          <p className="mt-3 text-splash-200">Last updated: March 1, 2026</p>
        </div>
      </section>

      {/* Content */}
      <section className="mx-auto max-w-3xl px-6 py-12">
        <p className="mb-4 leading-relaxed text-muted-foreground">
          These Terms of Service (&quot;Terms&quot;) govern your access to
          and use of SplashSphere, a car wash management platform operated
          by LezanobTech (&quot;we,&quot; &quot;us,&quot; or
          &quot;our&quot;). By creating an account or using our services,
          you agree to be bound by these Terms.
        </p>

        {/* Acceptance of Terms */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          1. Acceptance of Terms
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          By registering for an account, accessing, or using the
          SplashSphere platform, you confirm that you have read, understood,
          and agree to these Terms and our{' '}
          <Link
            href="/privacy"
            className="text-splash-600 underline hover:text-splash-700"
          >
            Privacy Policy
          </Link>
          . If you are using SplashSphere on behalf of a business or
          organization, you represent that you have the authority to bind
          that entity to these Terms.
        </p>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          We reserve the right to update these Terms at any time. We will
          notify you of material changes via email or through the platform.
          Your continued use of SplashSphere after such changes constitutes
          acceptance of the updated Terms.
        </p>

        {/* Description of Service */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          2. Description of Service
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          SplashSphere is a cloud-based, multi-tenant software platform
          designed for Philippine car wash businesses. Our services include,
          but are not limited to:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>Point-of-sale (POS) transaction processing</li>
          <li>Vehicle queue management with real-time display</li>
          <li>Employee management with commission tracking and splitting</li>
          <li>Weekly payroll processing with automated calculations</li>
          <li>Multi-branch management and reporting</li>
          <li>Customer and vehicle records management</li>
          <li>Inventory and merchandise tracking</li>
          <li>Business analytics and reporting dashboards</li>
        </ul>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          We may modify, update, or discontinue features of the platform at
          any time. We will provide reasonable notice for any changes that
          materially affect your use of the service.
        </p>

        {/* Account Registration */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          3. Account Registration
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          To use SplashSphere, you must create an account and complete the
          onboarding process, which includes providing your business details
          and setting up your first branch. You agree to:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>Provide accurate and complete registration information.</li>
          <li>
            Keep your account credentials secure and confidential. You are
            responsible for all activity under your account.
          </li>
          <li>
            Notify us immediately at{' '}
            <a
              href="mailto:hello@splashsphere.ph"
              className="text-splash-600 underline hover:text-splash-700"
            >
              hello@splashsphere.ph
            </a>{' '}
            if you suspect unauthorized access to your account.
          </li>
          <li>
            Not share your account with anyone outside your organization.
            You may invite team members using the built-in invitation
            system.
          </li>
        </ul>

        {/* Subscription & Billing */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          4. Subscription & Billing
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          SplashSphere offers multiple subscription plans with different
          feature sets and usage limits. By selecting a plan, you agree to
          the following:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            <strong>Free Trial:</strong> New accounts receive a 14-day free
            trial with access to all features. No payment information is
            required during the trial. At the end of the trial, you must
            select a paid plan to continue using the service.
          </li>
          <li>
            <strong>Monthly Billing:</strong> Subscriptions are billed
            monthly in Philippine Pesos (PHP). Payment is due at the start
            of each billing cycle.
          </li>
          <li>
            <strong>Plan Changes:</strong> You may upgrade your plan at any
            time. Upgrades take effect immediately and are prorated for the
            remainder of the billing cycle. Downgrades take effect at the
            start of the next billing cycle, provided your current usage is
            within the limits of the new plan.
          </li>
          <li>
            <strong>Cancellation:</strong> You may cancel your subscription
            at any time. Cancellation takes effect immediately. We do not
            offer refunds for partial billing periods. Your data will be
            retained for 90 days after cancellation, during which you may
            reactivate your account.
          </li>
          <li>
            <strong>Failed Payments:</strong> If a payment fails, we will
            attempt to collect payment for up to 14 days. If payment cannot
            be collected, your account will be suspended until the
            outstanding balance is settled.
          </li>
        </ul>

        {/* Acceptable Use */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          5. Acceptable Use
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          You agree to use SplashSphere only for lawful purposes and in
          accordance with these Terms. You shall not:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            Use the platform for any activity that violates Philippine law
            or any applicable jurisdiction.
          </li>
          <li>
            Attempt to gain unauthorized access to other users&apos;
            accounts, data, or any part of the platform&apos;s
            infrastructure.
          </li>
          <li>
            Upload malicious code, viruses, or any content designed to
            disrupt or damage the platform.
          </li>
          <li>
            Use automated tools (bots, scrapers) to access the platform
            without our prior written consent.
          </li>
          <li>
            Resell, sublicense, or redistribute access to the platform
            without authorization.
          </li>
          <li>
            Enter false or misleading data into the system to manipulate
            reports, payroll, or financial records.
          </li>
        </ul>

        {/* Intellectual Property */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          6. Intellectual Property
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          The SplashSphere platform, including its source code, design,
          logos, documentation, and all related intellectual property, is
          owned by LezanobTech and protected by Philippine intellectual
          property laws and international treaties.
        </p>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          Your subscription grants you a limited, non-exclusive,
          non-transferable, revocable license to use the platform for your
          internal business purposes. This license does not grant you
          ownership of any intellectual property.
        </p>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          You retain ownership of all data you enter into SplashSphere.
          You grant us a limited license to process, store, and display
          your data solely to provide the service.
        </p>

        {/* Limitation of Liability */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          7. Limitation of Liability
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          To the maximum extent permitted by Philippine law, SplashSphere
          and LezanobTech shall not be liable for any indirect, incidental,
          special, consequential, or punitive damages, including but not
          limited to loss of profits, data, business opportunities, or
          goodwill, arising out of or related to your use of the platform.
        </p>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          Our total aggregate liability for any claims arising under these
          Terms shall not exceed the amount you have paid to us in
          subscription fees during the twelve (12) months preceding the
          claim.
        </p>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          The platform is provided &quot;as is&quot; and &quot;as
          available.&quot; We do not warrant that the service will be
          uninterrupted, error-free, or free of harmful components. We make
          no guarantees regarding uptime, although we strive to maintain
          high availability.
        </p>

        {/* Termination */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          8. Termination
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          You may terminate your account at any time by contacting us or
          using the cancellation feature in the platform. We may suspend or
          terminate your account if:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>You violate these Terms or our Acceptable Use policy.</li>
          <li>
            Your subscription payment is overdue for more than 14 days.
          </li>
          <li>
            We are required to do so by law or a valid legal order.
          </li>
          <li>
            We reasonably believe your account poses a security risk to
            the platform or other users.
          </li>
        </ul>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          Upon termination, your right to access the platform ceases
          immediately. We will retain your data for 90 days to allow for
          data export or account reactivation, after which it will be
          permanently deleted in accordance with our Privacy Policy.
        </p>

        {/* Governing Law */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          9. Governing Law
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          These Terms shall be governed by and construed in accordance with
          the laws of the Republic of the Philippines, without regard to
          conflict of law principles. Any disputes arising from or relating
          to these Terms or your use of SplashSphere shall be subject to
          the exclusive jurisdiction of the courts of Metro Manila,
          Philippines.
        </p>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          Before filing any legal claim, both parties agree to attempt
          resolution through good-faith negotiation for a period of at
          least thirty (30) days.
        </p>

        {/* Contact */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          10. Contact
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          If you have any questions about these Terms of Service, please
          contact us:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            <strong>Email:</strong>{' '}
            <a
              href="mailto:hello@splashsphere.ph"
              className="text-splash-600 underline hover:text-splash-700"
            >
              hello@splashsphere.ph
            </a>
          </li>
          <li>
            <strong>Company:</strong> LezanobTech
          </li>
        </ul>

        {/* Back to Home */}
        <div className="mt-12 border-t border-border pt-8">
          <Link
            href="/"
            className="text-splash-600 underline hover:text-splash-700"
          >
            &larr; Back to Home
          </Link>
        </div>
      </section>
    </>
  )
}
