<template>
    <nav class="q-main-navigation">
        <div class="q-main-navigation--item">
            <button type="button" class="q-button q-button__round" title="Main menu">
                <svg class="q-icon">
                    <use xlink:href="#icon-menu"></use>
                </svg>
            </button>
            <div class="q-drop-down-menu">
                <RouterLink to="/" class="q-menu-item">
                    <div class="q-menu-item--icon">
                        <svg class="q-icon">
                            <use xlink:href="#icon-timesheet"></use>
                        </svg>
                    </div>
                    <div class="q-menu-item--content">
                        <div class="q-menu-item--title">Timesheet</div>
                        <div class="q-menu-item--sub-title">Register activity for a day.</div>
                    </div>
                </RouterLink>

                <RouterLink to="/admin/users" class="q-menu-item">
                    <div class="q-menu-item--icon">
                        <svg class="q-icon">
                            <use xlink:href="#icon-users"></use>
                        </svg>
                    </div>
                    <div class="q-menu-item--content">
                        <div class="q-menu-item--title">Users</div>
                        <div class="q-menu-item--sub-title">Manage registered users.</div>
                    </div>
                </RouterLink>

                <RouterLink to="/about" class="q-menu-item">
                    <div class="q-menu-item--icon">
                        <svg class="q-icon">
                            <use xlink:href="#icon-info"></use>
                        </svg>
                    </div>
                    <div class="q-menu-item--content">
                        <div class="q-menu-item--title">About</div>
                        <div class="q-menu-item--sub-title">Test page.</div>
                    </div>
                </RouterLink>
            </div>
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
import {RouterLink} from "vue-router";
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
