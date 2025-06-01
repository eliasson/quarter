<template>
    <nav class="q-main-navigation">
        <div class="q-main-navigation--item">
            <drop-down-menu
                trigger-icon="icon-menu"
                trigger-title="Main menu"
                :items="applicationMenuItems">
            </drop-down-menu>
        </div>

        <div class="q-main-navigation--item">
            <button type="button" class="q-button q-button__round" title="Main menu">
                <svg class="q-icon">
                    <use xlink:href="#icon-user"></use>
                </svg>
            </button>
        </div>
    </nav>
</template>

<script setup lang="ts">
import DropDownMenu from "@/components/DropDownMenu.vue"
import { MenuItem } from "@/models/ui.ts"
import { computed } from "vue"
import { AppPaths } from "@/router"

// The menu items should be based on the users access rights. Not everyone should see the administrative options.
const applicationMenuItems = computed(() => [
    new MenuItem("Timesheets", "Register activity for a day.", "icon-timesheet", AppPaths.Timesheets),
    new MenuItem("Projects", "Manage your projects and activities.", "icon-info", AppPaths.Projects),
    new MenuItem("Users", "Manage registered users.", "icon-user", AppPaths.Users),
])

</script>

<style scoped>
    .q-main-navigation {
        display: flex;
        justify-content:  space-between;
        padding: 1rem;
    }

    .q-main-navigation--item:first-of-type {
        justify-self: start;
    }

    .q-main-navigation--item:last-of-type {
        justify-self: end;
    }
</style>
