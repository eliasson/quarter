import { ViewObject } from "@/utils/test-utils/view-models/ViewObject.ts"
import type { VueWrapper } from "@vue/test-utils"
import { ButtonViewObject } from "@/utils/test-utils/view-models/ButtonViewObject.ts"


export class DropDownMenuViewObject extends ViewObject {
    constructor(
        w: VueWrapper,
        testId: string = "drop-down-menu") {
        const target = w.find(`[data-testid=${testId}]`)
        super(target)
    }

    trigger(): ButtonViewObject {
        return new ButtonViewObject(this.wrapper, "trigger")
    }

    /** Get the modal part of the drop-down menu. */
    modal(): ViewObject {
        const modal = this.wrapper.find("[data-testid=drop-down-menu-modal]")
        return new ViewObject(modal)
    }

    /** Get the menu items existing in this menu. */
    items(): DropDownMenuItemViewObject[] {
        const result = this.wrapper.findAll("[data-testid=menu-item]")
        return result
            .map((w: VueWrapper) => new DropDownMenuItemViewObject(w))
    }
}

export class DropDownMenuItemViewObject extends ViewObject {
    constructor(w: VueWrapper) {
        super(w)
    }

    icon(): IconViewObject {
        return new IconViewObject(this.wrapper.find("[data-testid=menu-icon]"))
    }

    title(): string {
        return this.wrapper.find("[data-testid=menu-title]").text()
    }

    subTitle(): string {
        return this.wrapper.find("[data-testid=menu-sub-title]").text()
    }
}

export class IconViewObject extends ViewObject {
    constructor(w: VueWrapper) {
        super(w)
    }

    icon(): string {
        // The XML namespace is ignored when getting xlink:href attribute
        return String(this.wrapper.find("use").attributes("href"))
    }
}
