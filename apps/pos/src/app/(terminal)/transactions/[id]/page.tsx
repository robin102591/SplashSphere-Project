interface TransactionDetailPageProps {
  params: Promise<{ id: string }>
}

export default async function TransactionDetailPage({ params }: TransactionDetailPageProps) {
  const { id } = await params

  return (
    <div className="p-4 max-w-2xl mx-auto space-y-6">
      <h1 className="text-xl font-bold text-white">Transaction #{id}</h1>
      <div className="rounded-xl border border-dashed border-gray-700 p-12 text-center">
        <p className="text-gray-500">Transaction detail &amp; receipt coming soon</p>
      </div>
    </div>
  )
}
