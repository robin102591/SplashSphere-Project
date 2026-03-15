import type { NextConfig } from 'next'

const nextConfig: NextConfig = {
  experimental: {
    reactCompiler: true,
  } as Record<string, unknown>,
}

export default nextConfig
