import { describe, it, expect, beforeEach } from 'vitest'

import { setActivePinia, createPinia, type Store} from 'pinia'
import { useCurrentUser} from "@/stores/use-current-user.ts"
import {AnonymousUserIdentity} from "@/models/user.ts";

describe('useCurrentUser', () => {
    beforeEach(() => {
        setActivePinia(createPinia())
    })

    describe("initially", () => {
        let composable: ReturnType<typeof useCurrentUser>

        beforeEach(() => {
            composable = useCurrentUser()
        })

        it("should not be initialized", () => {
            expect((composable as any).isInitialized).toBe(false)
        })

        it("should have the default anonymous user as current user", () => {
            expect(composable.currentUser).toEqual(AnonymousUserIdentity)
        })

        describe("when initializing", () => {
            beforeEach(async () => {
                // TODO Fake the api client
                await composable.initialize()
            })
            it("should be initialized", () => {
                expect((composable as any).isInitialized).toBe(true)
            })
        })
    })
})
