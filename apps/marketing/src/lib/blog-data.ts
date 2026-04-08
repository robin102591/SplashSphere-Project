export type BlogPost = {
  slug: string
  title: string
  excerpt: string
  date: string
  author: string
  readTime: string
  content: string
}

const blogPosts: BlogPost[] = [
  {
    slug: 'how-to-calculate-car-wash-commissions',
    title:
      'How to Calculate Car Wash Employee Commissions (Without Losing Your Mind)',
    excerpt:
      'Commission splitting across multiple employees, vehicle types, and service tiers is one of the biggest headaches for Philippine car wash owners. Here is how to get it right.',
    date: '2026-03-15',
    author: 'SplashSphere Team',
    readTime: '5 min read',
    content: `
      <p>If you run a car wash in the Philippines, you know the drill: most of your employees are commission-based. They earn a percentage or fixed amount for every vehicle they service. Sounds simple enough — until you realize that three guys washed the same SUV, the pricing is different from a sedan, and one of them also helped with the interior detailing package.</p>

      <h2>The Commission Calculation Challenge</h2>
      <p>Philippine car washes typically price services by vehicle type (sedan, SUV, van, truck) and sometimes by size within that type. Commission rates follow the same matrix — an exterior wash on a sedan might earn 30 pesos per employee, while the same wash on a large SUV could be 50 pesos. When multiple employees share a service, the commission splits evenly among them. Tracking all of this on paper or a basic spreadsheet becomes a nightmare, especially when you have 8-10 employees across different shifts.</p>

      <h2>Common Mistakes That Cost You Money</h2>
      <ul>
        <li><strong>Rounding errors that accumulate</strong> — splitting 100 pesos three ways gives 33.33 each, but where does the extra centavo go? Over hundreds of transactions per week, these add up.</li>
        <li><strong>Forgetting to account for packages</strong> — bundled services have their own pricing and commission rates that differ from individual services.</li>
        <li><strong>Manual tallying at payroll time</strong> — spending hours every week adding up commissions from a logbook is not just tedious, it is error-prone.</li>
        <li><strong>Employee disputes</strong> — without clear records, disagreements about who worked on which vehicle are common and hurt morale.</li>
      </ul>

      <h2>How SplashSphere Handles It</h2>
      <p>SplashSphere automates the entire commission calculation. You set up your pricing matrix and commission matrix once — per service, per vehicle type, per size. When a transaction is created, the system automatically looks up the correct commission rate, splits it evenly among assigned employees (with proper rounding using banker's rounding), and tracks everything down to the centavo. At payroll time, commissions are already tallied and ready for review.</p>

      <p>Ready to stop calculating commissions by hand? <a href="/pricing">Try SplashSphere free for 14 days</a> and see how much time you save on your next weekly payroll.</p>
    `,
  },
  {
    slug: '5-signs-your-car-wash-needs-a-pos',
    title: '5 Signs Your Car Wash Needs a POS System',
    excerpt:
      'Still using a notebook and calculator at your car wash? Here are five clear signals it is time to upgrade to a proper POS system.',
    date: '2026-03-01',
    author: 'SplashSphere Team',
    readTime: '4 min read',
    content: `
      <p>Many Philippine car wash owners start out with a simple setup: a logbook for transactions, a calculator for totals, and a notebook to track employee commissions. It works fine when you have one branch and a handful of employees. But as your business grows, the cracks start to show. Here are five signs it is time to upgrade.</p>

      <h2>1. Payroll Takes You an Entire Day</h2>
      <p>If you are spending every Monday morning hunched over a logbook, tallying up commissions and daily rates for each employee, that is time you could spend growing your business. A POS system tracks every transaction and automatically calculates what each employee earned.</p>

      <h2>2. You Suspect Cash Leakage</h2>
      <p>Without a system that records every transaction in real time, it is nearly impossible to know if all cash collected actually makes it to the register. Shift management with opening and closing cash counts gives you accountability without micromanaging.</p>

      <h2>3. Customers Are Asking for GCash</h2>
      <p>Digital payments are no longer optional in the Philippines. If you are turning away customers because you can only accept cash, you are leaving money on the table. A POS system lets you record GCash, Maya, and card payments alongside cash — and track them all in one place.</p>

      <h2>4. You Have No Idea Which Services Make the Most Money</h2>
      <p>Is your interior detailing package actually profitable, or are you losing money on it after commissions? Without data, you are guessing. A POS system with built-in reports shows you revenue by service, profit margins, and peak hours so you can make informed decisions.</p>

      <h2>5. You Are Opening a Second Branch</h2>
      <p>Managing one branch from memory is possible. Managing two is not. You need centralized reporting, consistent pricing across locations, and the ability to check on any branch from your phone. This is where multi-branch management becomes essential.</p>

      <p>If any of these sound familiar, it might be time to try a purpose-built car wash POS. <a href="/pricing">SplashSphere offers a free 14-day trial</a> — no credit card required.</p>
    `,
  },
  {
    slug: 'gcash-for-car-wash',
    title:
      'GCash for Car Wash: How to Accept Digital Payments at Your Shop',
    excerpt:
      'GCash is the most popular e-wallet in the Philippines. Here is how to start accepting it at your car wash and why it matters for your bottom line.',
    date: '2026-02-15',
    author: 'SplashSphere Team',
    readTime: '3 min read',
    content: `
      <p>With over 90 million registered users, GCash has become the default digital wallet in the Philippines. Your customers already use it for everything from grocery shopping to paying bills. If your car wash does not accept GCash yet, you are making it harder for people to pay you — and some will just go to the competitor down the street who does.</p>

      <h2>Setting Up GCash at Your Car Wash</h2>
      <p>The easiest way to start is with a personal GCash QR code displayed at your cashier counter. Customers scan, pay, and show the confirmation. For higher volume, consider registering as a GCash merchant through the GCash Business portal to get lower transaction fees and automatic reconciliation. Either way, the key is training your cashier to verify every payment before marking the transaction as paid.</p>

      <h2>The Tracking Problem</h2>
      <p>Here is where most car wash owners run into trouble: at the end of the day, you have cash in the register and GCash payments in your phone, and you need to reconcile both against your transactions. If you are doing this manually, it is easy to miss a payment or double-count one. This gets even messier when you have multiple cashiers handling shifts throughout the day.</p>

      <h2>How SplashSphere Simplifies Digital Payments</h2>
      <p>SplashSphere's POS supports multiple payment methods per transaction. Your cashier selects "GCash" as the payment type, enters the reference number, and the system records it alongside the transaction. At shift close, GCash totals are separated from cash totals automatically. The shift report shows exactly how much was collected via each method, making reconciliation a five-minute task instead of a thirty-minute headache.</p>

      <p>Want to see how easy digital payment tracking can be? <a href="/pricing">Start your free SplashSphere trial today</a> and modernize your car wash operations.</p>
    `,
  },
]

export function getBlogPosts(): BlogPost[] {
  return blogPosts.sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
  )
}

export function getBlogPost(slug: string): BlogPost | undefined {
  return blogPosts.find((post) => post.slug === slug)
}
