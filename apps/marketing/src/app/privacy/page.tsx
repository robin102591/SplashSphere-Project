import type { Metadata } from 'next'
import Link from 'next/link'

export const metadata: Metadata = {
  title: 'Privacy Policy',
}

export default function PrivacyPolicyPage() {
  return (
    <>
      {/* Hero Banner */}
      <section className="bg-splash-900 py-12 text-white">
        <div className="mx-auto max-w-3xl px-6 text-center">
          <h1 className="text-4xl font-bold tracking-tight">Privacy Policy</h1>
          <p className="mt-3 text-splash-200">Last updated: March 1, 2026</p>
        </div>
      </section>

      {/* Content */}
      <section className="mx-auto max-w-3xl px-6 py-12">
        <p className="mb-4 leading-relaxed text-muted-foreground">
          SplashSphere (&quot;we,&quot; &quot;us,&quot; or &quot;our&quot;) is a
          product of LezanobTech, a company registered in the Philippines.
          This Privacy Policy describes how we collect, use, store, and
          protect your personal information when you use the SplashSphere
          platform, including our web applications, APIs, and related
          services. We are committed to complying with Republic Act No.
          10173, also known as the Data Privacy Act of 2012 (DPA), and its
          Implementing Rules and Regulations.
        </p>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          By accessing or using SplashSphere, you acknowledge that you have
          read and understood this Privacy Policy and consent to the
          collection and processing of your personal data as described
          herein.
        </p>

        {/* Information We Collect */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          Information We Collect
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          We collect information that you provide directly to us when you
          create an account, set up your business profile, or use our
          services. This includes:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            <strong>Account Information:</strong> Name, email address,
            phone number, and password (managed securely via our
            authentication provider).
          </li>
          <li>
            <strong>Business Information:</strong> Business name, branch
            addresses, business registration details, and contact
            information.
          </li>
          <li>
            <strong>Employee Data:</strong> Names, contact details,
            employment type, commission rates, daily rates, and attendance
            records as entered by the account owner.
          </li>
          <li>
            <strong>Customer and Vehicle Data:</strong> Customer names,
            contact details, vehicle plate numbers, makes, models, and
            service history as entered by your staff.
          </li>
          <li>
            <strong>Transaction Data:</strong> Service records, payment
            amounts, payment methods, timestamps, and receipt information.
          </li>
          <li>
            <strong>Usage Data:</strong> Browser type, IP address, device
            information, pages visited, and feature usage patterns collected
            automatically when you interact with our platform.
          </li>
        </ul>

        {/* How We Use Your Information */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          How We Use Your Information
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          We process your personal information for the following purposes,
          each with a lawful basis under the DPA:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            <strong>Service Delivery:</strong> To operate, maintain, and
            improve the SplashSphere platform, including transaction
            processing, payroll calculations, queue management, and
            reporting.
          </li>
          <li>
            <strong>Account Management:</strong> To create and manage your
            account, authenticate your identity, and provide customer
            support.
          </li>
          <li>
            <strong>Billing:</strong> To process subscription payments,
            generate invoices, and manage your billing history.
          </li>
          <li>
            <strong>Communication:</strong> To send transactional emails
            (e.g., receipts, payroll summaries), service announcements, and,
            with your consent, promotional materials.
          </li>
          <li>
            <strong>Analytics:</strong> To understand how our platform is
            used and to develop new features, improve performance, and
            enhance the user experience.
          </li>
          <li>
            <strong>Legal Compliance:</strong> To comply with applicable
            laws, regulations, and legal processes.
          </li>
        </ul>

        {/* Data Storage & Security */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          Data Storage & Security
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          Your data is stored on secure servers with industry-standard
          encryption at rest and in transit. We implement technical and
          organizational measures to protect your personal information
          against unauthorized access, alteration, disclosure, or
          destruction. These measures include:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>TLS/SSL encryption for all data transmitted between your browser and our servers.</li>
          <li>AES-256 encryption for sensitive data at rest.</li>
          <li>Role-based access controls and multi-tenant data isolation ensuring your business data is never accessible to other tenants.</li>
          <li>Regular automated backups with secure off-site storage.</li>
          <li>Periodic security assessments and vulnerability testing.</li>
        </ul>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          While we strive to use commercially acceptable means to protect
          your personal information, no method of transmission over the
          Internet or method of electronic storage is 100% secure. We
          cannot guarantee absolute security.
        </p>

        {/* Third-Party Services */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          Third-Party Services
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          We use the following third-party service providers to operate
          SplashSphere. Each provider has its own privacy policy governing
          the use of your information:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            <strong>Clerk</strong> &mdash; Authentication and user
            management. Clerk processes your email address, name, and login
            credentials to provide secure sign-in functionality.
          </li>
          <li>
            <strong>Payment Processors</strong> &mdash; We integrate with
            third-party payment gateways to process subscription payments.
            We do not store your full credit card number or payment
            credentials on our servers. Payment information is handled
            directly by the processor in compliance with PCI DSS standards.
          </li>
          <li>
            <strong>Cloud Infrastructure Providers</strong> &mdash; Our
            servers and databases are hosted on reputable cloud platforms
            with data centers that maintain industry certifications.
          </li>
        </ul>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          We only share the minimum amount of data necessary for each
          third-party service to function. We do not sell your personal
          information to any third party.
        </p>

        {/* Your Rights Under RA 10173 */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          Your Rights Under RA 10173
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          As a data subject under the Philippine Data Privacy Act of 2012,
          you have the following rights:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            <strong>Right to Be Informed:</strong> You have the right to be
            informed of the collection and processing of your personal
            data, including the purposes, scope, and method of processing.
          </li>
          <li>
            <strong>Right to Access:</strong> You may request access to your
            personal data held by SplashSphere, including information about
            how it has been processed.
          </li>
          <li>
            <strong>Right to Object:</strong> You may object to the
            processing of your personal data, including processing for
            direct marketing, automated processing, or profiling.
          </li>
          <li>
            <strong>Right to Erasure or Blocking:</strong> You may request
            the removal or blocking of your personal data from our systems,
            subject to legal and contractual retention requirements.
          </li>
          <li>
            <strong>Right to Rectification:</strong> You may request the
            correction of inaccurate or incomplete personal data.
          </li>
          <li>
            <strong>Right to Data Portability:</strong> You may request a
            copy of your personal data in a structured, commonly used, and
            machine-readable format.
          </li>
          <li>
            <strong>Right to File a Complaint:</strong> You may file a
            complaint with the National Privacy Commission (NPC) if you
            believe your data privacy rights have been violated.
          </li>
        </ul>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          To exercise any of these rights, please contact us at{' '}
          <a
            href="mailto:hello@splashsphere.ph"
            className="text-splash-600 underline hover:text-splash-700"
          >
            hello@splashsphere.ph
          </a>
          . We will respond to your request within thirty (30) days.
        </p>

        {/* Data Retention */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          Data Retention
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          We retain your personal data for as long as your account is active
          or as needed to provide you with our services. If you close your
          account or request deletion of your data, we will delete or
          anonymize your personal information within ninety (90) days,
          except where retention is required by law (e.g., tax records,
          financial transaction logs) or legitimate business purposes such
          as resolving disputes.
        </p>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          Transaction data and payroll records may be retained for up to
          seven (7) years in compliance with Philippine tax and labor
          regulations. Aggregated, anonymized data that cannot be used to
          identify you may be retained indefinitely for analytics and
          service improvement.
        </p>

        {/* Cookies */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          Cookies
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          SplashSphere uses cookies and similar technologies to maintain
          your session, remember your preferences (such as language
          settings), and analyze platform usage. The types of cookies we
          use include:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            <strong>Essential Cookies:</strong> Required for authentication,
            session management, and core platform functionality. These
            cannot be disabled.
          </li>
          <li>
            <strong>Preference Cookies:</strong> Store your language
            selection and display preferences (e.g., the{' '}
            <code className="rounded bg-gray-100 px-1.5 py-0.5 font-mono text-sm">
              NEXT_LOCALE
            </code>{' '}
            cookie for English/Filipino).
          </li>
          <li>
            <strong>Analytics Cookies:</strong> Help us understand how the
            platform is used so we can improve it. You may opt out of
            analytics cookies through your browser settings.
          </li>
        </ul>

        {/* Contact Us */}
        <h2 className="mt-10 mb-4 text-2xl font-bold text-foreground">
          Contact Us
        </h2>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          If you have any questions about this Privacy Policy or wish to
          exercise your data privacy rights, please contact us:
        </p>
        <ul className="mb-4 list-inside list-disc space-y-2 text-muted-foreground">
          <li>
            <strong>Data Controller:</strong> LezanobTech
          </li>
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
            <strong>Data Protection Officer:</strong> You may reach our DPO
            at{' '}
            <a
              href="mailto:dpo@splashsphere.ph"
              className="text-splash-600 underline hover:text-splash-700"
            >
              dpo@splashsphere.ph
            </a>
          </li>
        </ul>
        <p className="mb-4 leading-relaxed text-muted-foreground">
          You may also file a complaint with the{' '}
          <strong>National Privacy Commission</strong> at{' '}
          <a
            href="https://www.privacy.gov.ph"
            target="_blank"
            rel="noopener noreferrer"
            className="text-splash-600 underline hover:text-splash-700"
          >
            www.privacy.gov.ph
          </a>
          .
        </p>

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
