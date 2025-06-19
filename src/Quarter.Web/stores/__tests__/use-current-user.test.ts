import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useCurrentUser} from "@/stores/use-current-user.ts"
import { AnonymousUserIdentity } from "@/models/user.ts"
import { FakeApiClient } from "@/utils/test-utils/fake-api-client.ts"

describe('useCurrentUser', () => {
    let apiClientMock: FakeApiClient

    beforeEach(() => {
        apiClientMock  = new FakeApiClient()
        setActivePinia(createPinia())
    })

    describe("initially", () => {
        let composable: ReturnType<typeof useCurrentUser>

        beforeEach(() => {
            composable = useCurrentUser(apiClientMock)
        })

        it("should not be initialized", () => {
            expect((composable as any).isInitialized).toBe(false)
        })

        it("should have the default anonymous user as current user", () => {
            expect(composable.currentUser).toEqual(AnonymousUserIdentity)
        })

        describe("when initializing", () => {
            beforeEach(async () => {
                apiClientMock.getCurrentUserResponse.resolveWith(AnonymousUserIdentity)
                await composable.initialize()
            })

            it("should be initialized", () => {
                expect((composable as any).isInitialized).toBe(true)
            })
        })
    })
})
