import { describe, it, expect, beforeEach } from "vitest"
import { ClientApi, type IClientApi } from "@/utils/client-api.ts"

describe("ClientApi", () => {
    let client: IClientApi

    beforeEach(() => {
        client = new ClientApi()
    })

    describe("getCurrentUser", () => {
        beforeEach(() => {
            void client.getCurrentUser()
        })

        it("should GET user from expected end-point", () => {
            expect(true).toBe(false)
        })
    })
})
