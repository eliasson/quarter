import type { InjectionKey } from "@vue/runtime-core"
import type { IApiClient } from "@/utils/api-client.ts"
import { inject } from "vue"
import { raiseError } from "@/utils/common.ts"

export type QInjectionKey<T> = InjectionKey<T> & symbol

/** The API Client used for all backend communication. */
export const ApiClientKey: QInjectionKey<IApiClient> = Symbol("ApiClient") as InjectionKey<IApiClient>

export function injectOrThrow<T>(key: QInjectionKey<T>): T {
    const dep = inject(key)
    return dep as T ?? raiseError("Dependency not found: " + key.toString())
}
