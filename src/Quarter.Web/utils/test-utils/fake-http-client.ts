import type { IHttpClient } from "@/utils/http-client.ts"

export class FakeHttpClient implements IHttpClient {
    public readonly getResponses: Map<string, Promise<any>>

    constructor() {
        this.getResponses = new Map<string, Promise<any>>()
    }

    get<TT>(url: string): Promise<TT> {
        const response = this.getResponses.get(url)
        if (response === undefined) {
            return Promise.reject("Fake HTTP Client not setup properly!")
        }

        return response
    }
}
