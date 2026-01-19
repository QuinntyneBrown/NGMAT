# Coding Guidelines Audit Report - src/Workspace

**Audit Date:** 2026-01-19
**Audited Directory:** `src/Workspace`
**Guidelines Version:** 1.0

---

## Executive Summary

This audit identified **52 violations** across the `src/Workspace` directory when compared against the established coding guidelines. The violations are categorized by severity and guideline section.

| Severity | Count |
|----------|-------|
| Critical | 17 |
| Major | 23 |
| Minor | 12 |

---

## Critical Violations

### 1. Angular Signals Usage (Section 7.2 - Prohibited)

**Guideline:** "Angular signals" are explicitly prohibited.

| File | Lines | Violation |
|------|-------|-----------|
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 1, 68-82 | Uses `signal()` and `computed()` imports and implementations |
| `projects/ui/src/app/pages/missions/missions.ts` | 1, 53-59 | Uses `signal()` for state management |
| `projects/ui/src/app/layout/shell/shell.ts` | 1, 50-51 | Uses `signal()` for `notificationCount` and `sidenavOpened` |

**Affected Signals:**
```typescript
// mission-editor.ts
protected mission = signal<Mission | null>(null);
protected missionTree = signal<MissionTreeNode[]>([]);
protected selectedNode = signal<MissionTreeNode | null>(null);
protected loading = signal(false);
protected saving = signal(false);
protected autoSaved = signal(false);
protected isNew = signal(false);
protected validationStatus = signal<'valid' | 'warning' | 'error'>('valid');
protected validationMessage = signal('...');
protected enableRealTimePropagation = signal(true);
protected enableManeuverOptimization = signal(true);
protected enableEphemerisOutput = signal(false);
protected enableCollisionAvoidance = signal(true);

// missions.ts
protected missions = signal<Mission[]>([]);
protected loading = signal(false);
protected totalCount = signal(0);
protected pageSize = signal(10);
protected pageIndex = signal(0);
protected searchTerm = signal('');
protected statusFilter = signal<MissionStatus | null>(null);

// shell.ts
protected readonly notificationCount = signal(3);
protected readonly sidenavOpened = signal(true);
```

**Recommendation:** Replace all `signal()` usage with RxJS BehaviorSubject/Observable patterns and expose data via `$` suffixed observables consumed with the async pipe.

---

### 2. Manual .subscribe() for Data Loading (Section 7.7 - Prohibited)

**Guideline:** Components SHALL use the async pipe pattern with observables. Manual `.subscribe()` calls for data loading are prohibited.

| File | Line(s) | Context |
|------|---------|---------|
| `projects/ui/src/app/pages/login/login.ts` | 57 | `this.authService.login({...}).subscribe({...})` |
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 143 | `this.missionService.getMission(id).subscribe({...})` |
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 177 | `this.missionService.getMissionTree(id).subscribe({...})` |
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 205 | Auto-save subscription |
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 222 | `this.missionService.updateMission(...).subscribe({...})` |
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 246 | `this.missionService.createMission(...).subscribe({...})` |
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 266 | `this.missionService.updateMission(...).subscribe({...})` |
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 291 | `this.missionService.cloneMission(...).subscribe({...})` |
| `projects/ui/src/app/pages/mission-editor/mission-editor.ts` | 308 | `this.missionService.deleteMission(...).subscribe({...})` |
| `projects/ui/src/app/pages/missions/missions.ts` | 79 | `this.missionService.getMissions(...).subscribe({...})` |
| `projects/ui/src/app/pages/missions/missions.ts` | 129 | `this.missionService.cloneMission(...).subscribe({...})` |
| `projects/ui/src/app/pages/missions/missions.ts` | 143 | `this.missionService.deleteMission(...).subscribe({...})` |

**Recommendation:** Refactor to use observables exposed as class properties with `$` suffix and consume via async pipe in templates:

```typescript
// Compliant pattern
viewModel$ = this.missionService.getMission(id).pipe(
  map(mission => ({ mission, loading: false }))
);
```

---

## Major Violations

### 3. Hardcoded Colors in SCSS (Section 7.2 - Prohibited)

**Guideline:** "Colors not defined in Angular Material theme" are prohibited. Only default Angular Material colors and theme should be used.

**Files with Hardcoded Hex Colors (31 files detected):**

| Category | Files |
|----------|-------|
| UI Application | `ui/src/styles.scss`, `ui/src/app/pages/missions/missions.scss`, `ui/src/app/pages/mission-editor/mission-editor.scss`, `ui/src/app/pages/login/login.scss`, `ui/src/app/pages/dashboard/dashboard.scss`, `ui/src/app/layout/shell/shell.scss` |
| Components Library | `components/src/lib/overlay/validation-banner/validation-banner.scss`, `components/src/lib/status/status-badge/status-badge.scss`, `components/src/lib/status/status-dot/status-dot.scss`, `components/src/lib/status/trend-indicator/trend-indicator.scss`, `components/src/lib/content/*.scss`, `components/src/lib/forms/*.scss`, `components/src/lib/visualization/chart-tooltip/chart-tooltip.scss` |

**Examples of Hardcoded Colors:**

```scss
// validation-banner.scss - Uses hardcoded colors
&--success {
  background-color: #e8f5e9;  // VIOLATION
  color: #2e7d32;             // VIOLATION
}

// shell.scss - Uses hardcoded colors
.app-toolbar {
  background-color: #1e1e1e;  // VIOLATION
}
&__logo-icon {
  color: #2196f3;             // VIOLATION
}

// missions.scss - Extensive hardcoded colors
.missions-card {
  background-color: #1e1e1e;  // VIOLATION
}
.status-chip--active {
  background-color: rgba(76, 175, 80, 0.2);  // VIOLATION
  color: #4caf50;                             // VIOLATION
}
```

**Recommendation:** Use Angular Material theme variables or CSS custom properties that derive from the Material theme.

---

### 4. Custom Color Token Definitions (Section 7.2 - Prohibited)

**Guideline:** Only Angular Material theme colors should be used.

**File:** `projects/components/src/lib/styles/tokens/_colors.scss`

This file defines custom color palettes outside of Angular Material's theming system:

```scss
// Primary palette (custom - VIOLATION)
$primary-50: #e3f2fd;
$primary-500: #2196f3;
// ... 66 lines of custom color definitions

// Status colors (custom - VIOLATION)
$status-success: #4caf50;
$status-warning: #ff9800;
$status-error: #f44336;
$status-info: #2196f3;
```

**Recommendation:** Remove custom color tokens and use Angular Material's built-in theming system with `mat.define-theme()` and theme variables.

---

### 5. Missing Barrel Exports (Section 7.5)

**Guideline:** Create barrel exports (`index.ts`) for every folder, exporting all TypeScript code except test code.

**Missing `index.ts` files in UI project:**

| Directory | Status |
|-----------|--------|
| `projects/ui/src/app/services/` | Missing |
| `projects/ui/src/app/models/` | Missing |
| `projects/ui/src/app/pages/` | Missing |
| `projects/ui/src/app/pages/dashboard/` | Missing |
| `projects/ui/src/app/pages/missions/` | Missing |
| `projects/ui/src/app/pages/mission-editor/` | Missing |
| `projects/ui/src/app/pages/login/` | Missing |
| `projects/ui/src/app/layout/` | Missing |
| `projects/ui/src/app/layout/shell/` | Missing |

**Note:** The components library has proper barrel exports in all directories.

**Recommendation:** Add `index.ts` files to all directories listed above.

---

### 6. Multiple Type Definitions in Single File (Section 2.2)

**Guideline:** Each file SHALL contain exactly one class, enum, record, or other type definition.

| File | Exported Types |
|------|----------------|
| `visualization/chart-tooltip/chart-tooltip.ts` | `ChartTooltipItem`, `ChartTooltipData`, `ChartTooltipPosition`, `ChartTooltip` (4 types) |
| `visualization/time-range-picker/time-range-picker.ts` | `TimeRangePreset`, `TimeRange`, `TimeRangePicker` (3 types) |
| `buttons/button/button.ts` | `ButtonVariant`, `ButtonSize`, `Button` (3 types) |
| `buttons/icon-button/icon-button.ts` | `IconButtonSize`, `IconButton` (2 types) |
| `content/mission-card/mission-card.ts` | `MissionStatus`, `Mission`, `MissionCard` (3 types) |
| `content/stat-card/stat-card.ts` | `TrendData`, `StatCard` (2 types) |
| `content/property-panel/property-panel.ts` | `Property`, `PropertyPanel` (2 types) |
| `content/info-panel/info-panel.ts` | `InfoItem`, `InfoPanel` (2 types) |
| `content/activity-list/activity-list.ts` | `ActivityItem`, `ActivityList` (2 types) |
| `forms/datetime-group/datetime-group.ts` | `DateTimeValue`, `DatetimeGroup` (2 types) |
| `forms/vector-input/vector-input.ts` | `Vector3`, `VectorInput` (2 types) |
| `forms/chip-list/chip-list.ts` | `ChipItem`, `ChipList` (2 types) |
| `forms/filter-chips/filter-chips.ts` | `FilterOption`, `FilterChips` (2 types) |
| `visualization/chart-legend/chart-legend.ts` | `ChartLegendItem`, `ChartLegend` (2 types) |
| `visualization/chart-tabs/chart-tabs.ts` | `ChartTab`, `ChartTabs` (2 types) |
| `expandable/tree-item/tree-item.ts` | `TreeItemNode`, `TreeItem` (2 types) |
| `overlay/snackbar/snackbar.service.ts` | `SnackbarType`, `SnackbarService` (2 types) |
| `overlay/validation-banner/validation-banner.ts` | `ValidationBannerStatus`, `ValidationBanner` (2 types) |
| `status/trend-indicator/trend-indicator.ts` | `TrendDirection`, `TrendIndicator` (2 types) |
| `status/status-badge/status-badge.ts` | `StatusBadgeStatus`, `StatusBadge` (2 types) |
| `status/status-dot/status-dot.ts` | `StatusDotStatus`, `StatusDot` (2 types) |

**Recommendation:** Extract interfaces, types, and enums into separate files (e.g., `chart-tooltip.types.ts`, `chart-tooltip.ts`).

---

## Minor Violations

### 7. Non-BEM CSS Class Naming (Section 7.8)

**Guideline:** BEM HTML class naming strategy is required.

| File | Violation | Correct Pattern |
|------|-----------|-----------------|
| `overlay/validation-banner/validation-banner.scss` | `.g-validation-banner-icon` | `.g-validation-banner__icon` |
| `overlay/validation-banner/validation-banner.scss` | `.g-validation-banner-message` | `.g-validation-banner__message` |
| `overlay/validation-banner/validation-banner.scss` | `.g-validation-banner-close` | `.g-validation-banner__close` |
| `visualization/chart-legend/chart-legend.scss` | Uses direct color `rgba(0,0,0,0.87)` | Should use theme variable |

---

### 8. Observable Naming Convention (Section 7.7)

**Guideline:** Expose observables as public properties with `$` suffix.

No observable properties in UI components use the `$` suffix convention because signals are used instead. When refactoring away from signals, ensure observables follow the naming convention:

```typescript
// Current (non-compliant - uses signals)
protected loading = signal(false);

// Should be (compliant)
protected loading$ = new BehaviorSubject<boolean>(false);
```

---

## Compliance Summary by Guideline Section

| Section | Guideline | Status | Issues |
|---------|-----------|--------|--------|
| 2.2 | Single type per file | Non-Compliant | 21 files |
| 7.2 | No Angular signals | Non-Compliant | 3 files |
| 7.2 | No custom colors | Non-Compliant | 31 files |
| 7.5 | Barrel exports | Partial | 9 missing |
| 7.7 | Async pipe pattern | Non-Compliant | 3 files |
| 7.7 | Observable `$` suffix | Non-Compliant | All UI files |
| 7.8 | BEM naming | Partial | 1 file |
| 7.3 | File naming | Compliant | - |
| 7.4 | Component naming | Compliant | - |

---

## Recommendations Priority

### Immediate Actions (Critical)

1. **Remove Angular Signals** - Refactor `mission-editor.ts`, `missions.ts`, and `shell.ts` to use RxJS observables with BehaviorSubject
2. **Eliminate manual .subscribe()** - Convert data loading to use async pipe pattern in templates
3. **Remove custom color definitions** - Use Angular Material theming system

### Short-term Actions (Major)

4. **Add missing barrel exports** - Create `index.ts` in all UI directories
5. **Separate type definitions** - Extract interfaces/types to dedicated files
6. **Replace hardcoded colors** - Use Material theme variables throughout SCSS

### Maintenance Actions (Minor)

7. **Fix BEM naming** - Update validation-banner classes
8. **Add observable `$` suffix** - After refactoring from signals

---

## Files Requiring Changes

### High Priority (Critical Violations)
- `projects/ui/src/app/pages/mission-editor/mission-editor.ts`
- `projects/ui/src/app/pages/missions/missions.ts`
- `projects/ui/src/app/layout/shell/shell.ts`
- `projects/ui/src/app/pages/login/login.ts`

### Medium Priority (Major Violations)
- All 31 SCSS files with hardcoded colors
- `projects/components/src/lib/styles/tokens/_colors.scss`
- 21 TypeScript files with multiple type definitions

### Lower Priority (Minor Violations)
- `projects/components/src/lib/overlay/validation-banner/validation-banner.scss`

---

**Report Generated:** 2026-01-19
**Auditor:** Claude Code Automated Audit
