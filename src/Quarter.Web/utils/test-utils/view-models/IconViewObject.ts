import { ViewObject } from "@/utils/test-utils/view-models/ViewObject.ts"
import type { VueWrapper } from "@vue/test-utils"

export class IconViewObject extends ViewObject {
    constructor(w: VueWrapper) {
        super(w)
    }

    icon(): string {
        // The XML namespace is ignored when getting xlink:href attribute
        return String(this.wrapper.find("use").attributes("href"))
    }
}
