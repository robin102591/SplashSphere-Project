'use client'

import { use, useEffect, useRef } from 'react'
import { useAuth } from '@clerk/nextjs'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Receipt } from '@splashsphere/types'
import { PaymentMethod } from '@splashsphere/types'

const PAYMENT_LABEL: Record<number, string> = {
  [PaymentMethod.Cash]:         'Cash',
  [PaymentMethod.GCash]:        'GCash / Maya',
  [PaymentMethod.CreditCard]:   'Credit Card',
  [PaymentMethod.DebitCard]:    'Debit Card',
  [PaymentMethod.BankTransfer]: 'Bank Transfer',
}

function fmt(amount: number) {
  return `P${Math.abs(amount).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}

function fmtDateTime(iso: string) {
  return new Date(iso).toLocaleString('en-PH', {
    month: 'short', day: 'numeric', year: 'numeric',
    hour: 'numeric', minute: '2-digit', hour12: true,
  })
}

interface Props {
  params: Promise<{ id: string }>
}

export default function ReceiptPage({ params }: Props) {
  const { id } = use(params)
  const { getToken } = useAuth()
  const hasPrinted = useRef(false)

  const { data: receipt, isLoading, error } = useQuery({
    queryKey: ['receipt', id],
    queryFn: async () => {
      const token = await getToken()
      return apiClient.get<Receipt>(`/transactions/${id}/receipt`, token ?? undefined)
    },
  })

  // Auto-print once data loads (if ?print=1 in URL)
  useEffect(() => {
    if (!receipt || hasPrinted.current) return
    const params = new URLSearchParams(window.location.search)
    if (params.get('print') === '1') {
      hasPrinted.current = true
      // Small delay to ensure render completes
      setTimeout(() => window.print(), 300)
    }
  }, [receipt])

  if (isLoading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <p className="text-gray-500 text-sm">Loading receipt...</p>
      </div>
    )
  }

  if (error || !receipt) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <p className="text-red-500 text-sm">Receipt not found.</p>
      </div>
    )
  }

  const totalPaid = receipt.payments.reduce((s, p) => s + p.amount, 0)
  const change = totalPaid - receipt.totalAmount

  return (
    <>
      {/* Screen toolbar — hidden in print */}
      <div className="print:hidden bg-gray-100 border-b p-3 flex items-center justify-between sticky top-0">
        <h1 className="text-sm font-semibold text-gray-700">Receipt: {receipt.transactionNumber}</h1>
        <div className="flex gap-2">
          <a
            href={`${process.env.NEXT_PUBLIC_API_URL}/api/v1/transactions/${id}/receipt/pdf`}
            target="_blank"
            rel="noopener noreferrer"
            className="px-3 py-2 text-xs rounded-lg bg-white border border-gray-300 text-gray-600 hover:bg-gray-50"
          >
            Download PDF
          </a>
          <button
            onClick={() => window.print()}
            className="px-4 py-2 text-xs rounded-lg bg-blue-600 text-white hover:bg-blue-700 font-medium"
          >
            Print Receipt
          </button>
        </div>
      </div>

      {/* Receipt content — visible in both screen (preview) and print */}
      <div id="receipt-content" className="max-w-[80mm] mx-auto py-6 px-4 font-mono text-[11px] leading-[1.4] text-black bg-white print:max-w-none print:py-0">
        <div className="receipt-header">
          <p className="receipt-brand text-center text-base font-bold">SplashSphere</p>
          <p className="receipt-branch text-center text-xs font-bold">{receipt.branch.name}</p>
          <p className="receipt-address text-center text-[10px] text-gray-600">{receipt.branch.address}</p>
          <p className="receipt-address text-center text-[10px] text-gray-600">{receipt.branch.contactNumber}</p>
        </div>

        <div className="receipt-separator border-t border-dashed border-black my-[6px]" />
        <p className="receipt-txn-number text-center text-[13px] font-bold tracking-wide">{receipt.transactionNumber}</p>
        <p className="receipt-date text-center text-[10px] text-gray-600 mt-0.5">{fmtDateTime(receipt.issuedAt)}</p>
        <div className="receipt-separator border-t border-dashed border-black my-[6px]" />

        <div className="receipt-info my-1 text-[11px]">
          <p>Plate: <strong>{receipt.vehicle.plateNumber}</strong></p>
          <p>Vehicle: {receipt.vehicle.vehicleTypeName} &middot; {receipt.vehicle.sizeName}</p>
          {receipt.customer && <p>Customer: {receipt.customer.name}</p>}
          <p>Cashier: {receipt.cashierName}</p>
        </div>

        <div className="receipt-separator border-t border-dashed border-black my-[6px]" />

        {/* Line items */}
        <div className="receipt-items my-1">
          {receipt.lineItems.map((item, i) => (
            <div key={i} className="receipt-line flex justify-between my-px">
              <span className="flex-1 truncate pr-2">
                {item.quantity > 1 ? `${item.name} x${item.quantity}` : item.name}
              </span>
              <span className="whitespace-nowrap text-right">{fmt(item.lineTotal)}</span>
            </div>
          ))}
        </div>

        <div className="receipt-separator border-t border-dashed border-black my-[6px]" />

        {/* Totals */}
        <div className="receipt-totals my-1">
          <div className="receipt-line flex justify-between my-px">
            <span>Subtotal</span><span>{fmt(receipt.subTotal)}</span>
          </div>
          {receipt.discountAmount > 0 && (
            <div className="receipt-line flex justify-between my-px">
              <span>Discount</span><span>-{fmt(receipt.discountAmount)}</span>
            </div>
          )}
          {receipt.taxAmount > 0 && (
            <div className="receipt-line flex justify-between my-px">
              <span>Tax</span><span>{fmt(receipt.taxAmount)}</span>
            </div>
          )}
          <div className="receipt-separator-solid border-t border-black my-1" />
          <div className="receipt-line receipt-total-line flex justify-between font-bold text-[13px] mt-1">
            <span>TOTAL</span><span>{fmt(receipt.totalAmount)}</span>
          </div>
        </div>

        {/* Payments */}
        {receipt.payments.length > 0 && (
          <>
            <div className="receipt-separator border-t border-dashed border-black my-[6px]" />
            <div className="receipt-payments my-1">
              {receipt.payments.map((p, i) => (
                <div key={i} className="receipt-line flex justify-between my-px">
                  <span>{PAYMENT_LABEL[p.method] ?? 'Payment'}</span>
                  <span>{fmt(p.amount)}</span>
                </div>
              ))}
              {change > 0.01 && (
                <div className="receipt-line receipt-total-line flex justify-between font-bold text-[13px] mt-1">
                  <span>CHANGE</span><span>{fmt(change)}</span>
                </div>
              )}
            </div>
          </>
        )}

        <div className="receipt-separator border-t border-dashed border-black my-[6px]" />
        <div className="receipt-footer text-center text-[11px] mt-1">
          <p>Thank you for choosing</p>
          <p className="font-bold">SplashSphere!</p>
        </div>

        {receipt.notes && (
          <p className="text-[9px] text-gray-500 text-center mt-2 italic">Notes: {receipt.notes}</p>
        )}
      </div>
    </>
  )
}
