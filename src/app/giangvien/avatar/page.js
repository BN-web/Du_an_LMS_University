"use client"

import { useRouter } from "next/navigation"
import Avatar from "../avatar"

export default function AvatarPage() {
  const router = useRouter()

  const handleBack = () => {
    router.back()
  }

  return <Avatar onBack={handleBack} />
}

