
/**
 * The browser fetch function signature, straight from lib.dom.d.ts.
 *
 * [MDN Reference](https://developer.mozilla.org/docs/Web/API/fetch)
 */
export type FetchFunction = (input: RequestInfo | URL, init?: RequestInit) => Promise<Response>

export const JsonContentType = "application/json";

export class HttpError extends Error {
    constructor(public readonly status: number, message?: string) {
        super(message)
    }
}

export class HttpClient {
    private readonly fetcher: FetchFunction

    constructor(fetcher?: FetchFunction) {
        this.fetcher = fetcher ?? window.fetch
    }

    async get<T>(url: string): Promise<T> {
        const response = await this.fetcher(url, {
            method: "GET",
        })

        if (!response.ok) {
            throw new HttpError(response.status, response.statusText)
        }

        const responseContentType = response.headers.get("content-type") ?? ""
        if (!responseContentType.startsWith(JsonContentType)) {
            throw new Error("Not a JSON response")
        }

        const json = await response.json()
        return json as T
    }
}
