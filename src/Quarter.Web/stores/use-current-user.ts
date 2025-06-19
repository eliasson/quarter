import { ref } from "vue"
import { defineStore } from "pinia"
import type { Ref } from "vue"
import { AnonymousUserIdentity, type UserIdentity } from "@/models/user.ts"
import type {IApiClient} from "@/utils/api-client.ts"

export interface UseCurrentUserState {
    /** The current user will always be set. If no user is logged in this will be an anonymous user object. */
    currentUser: Ref<UserIdentity>
    /** When initialized, the user has been fetched (or failed due to unauthenticated). */
    isInitialized: Ref<boolean>
    /** Try to initialize the store by fetching details from backend. */
    initialize(): Promise<void>
}

/*
The current user store should contain:

- The current user (loaded async) or default user.
- Indicate if authenticated or demo mode.
- Functions to check capabilities.
 */
export function useCurrentUser(apiClient: IApiClient) {
    return defineStore("currentUser", (): UseCurrentUserState => {
        const currentUser = ref<UserIdentity>(AnonymousUserIdentity)
        const isInitialized = ref<boolean>(false)

        async function initialize(): Promise<void> {
            currentUser.value = await apiClient.getCurrentUser()
            isInitialized.value = true
        }

        return { currentUser, isInitialized, initialize }
    })()
}
