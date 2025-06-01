import { ViewObject } from "@/utils/test-utils/view-models/ViewObject.ts"
import type { VueWrapper } from "@vue/test-utils"
import { ButtonViewObject } from "@/utils/test-utils/view-models/ButtonViewObject.ts"
import { IconViewObject } from "@/utils/test-utils/view-models/IconViewObject.ts"


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

    backDrop(): ViewObject {
        return new ViewObject(this.wrapper.find("[data-testid=back-drop]"))
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

    link(): string {
        return String(this.wrapper.attributes("href"))
    }
}
