// Type definitions for Google Identity Services

interface GoogleCredentialResponse {
  credential: string
  select_by: string
}

interface GoogleOneTapConfig {
  client_id: string
  callback: (response: GoogleCredentialResponse) => void
  auto_select?: boolean
  cancel_on_tap_outside?: boolean
  context?: "signin" | "signup" | "use"
  itp_support?: boolean
  state_cookie_domain?: string
  allowed_parent_origin?: string | string[]
  intermediate_iframe_close_callback?: () => void
}

interface GoogleAccounts {
  id: {
    initialize: (config: GoogleOneTapConfig) => void
    renderButton: (parent: HTMLElement, options: {
      theme?: "outline" | "filled_blue" | "filled_black"
      size?: "large" | "medium" | "small"
      width?: string | number
      text?: "signin_with" | "signup_with" | "continue_with" | "signin"
      shape?: "rectangular" | "pill" | "circle" | "square"
      logo_alignment?: "left" | "center"
      locale?: string
      click_listener?: () => void
    }) => void
    prompt: (momentNotification?: (notification: {
      isNotDisplayed: () => boolean
      isSkippedMoment: () => boolean
      isDismissedMoment: () => boolean
      getNotDisplayedReason: () => string
      getSkippedReason: () => string
      getDismissedReason: () => string
    }) => void) => void
    disableAutoSelect: () => void
    storeCredential: (credentials: { id: string; password: string }, callback?: () => void) => void
    cancel: () => void
    onGoogleLibraryLoad: () => void
    revoke: (hint: string, callback?: () => void) => void
  }
}

interface Window {
  google?: GoogleAccounts
}

