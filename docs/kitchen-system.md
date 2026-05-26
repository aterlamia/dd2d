# Kitchen System Setup

## Scene Wiring

Add these to the restaurant scene:

### 1. KitchenManager (Node)

```
KitchenManager (Node, script: KitchenManager.gd)
├── Chef0 (Node2D, script: Chef.gd)  — place near kitchen area
├── Chef1 (Node2D, script: Chef.gd)  — optional, add more as needed
└── ... (more chefs)
```

**KitchenManager exports:**
| Property | Value |
|---|---|
| `Menu` | Leave empty → uses 5 defaults (Burger, Salad, Pasta, Pizza, Steak) |
| `ChefPaths` | NodePath array pointing to each `Chef` child node |

### 2. Chef Entities

Each `Chef` node needs:

| Property | Value |
|---|---|
| `ChefData` | Create a new `ChefData` resource via Inspector → "New ChefData" |

**Recommended ChefData presets:**

| Chef | Speed | Presentation | Experience |
|---|---|---|---|
| Rookie | 0.8 | 1.0 | 1.0 |
| Line Cook | 1.0 | 1.5 | 2.0 |
| Sous Chef | 1.2 | 2.5 | 3.5 |
| Head Chef | 1.5 | 4.0 | 5.0 |

### 3. VisitorManager wiring

On the existing `VisitorManager` node:

| Property | Value |
|---|---|
| `Kitchen` | Drag the `KitchenManager` node |

**VisitorManager updates:**
- `RatingSubmitted` signal: Emitted when a visitor submits a rating after eating, passes `float finalQuality` and `string itemName`
- `TotalEarnings`: Read-only cumulative total of all visitor payments
- Static `Instance` property: Access VisitorManager globally via `VisitorManager.Instance`
- `RegisterPayment(float amount)`: Internal method to add to total earnings (called automatically by EatingState)

### 4. MenuManager (optional)

To override the 5 default recipes, create a `MenuManager` resource in the Filesystem dock (right-click → New Resource → MenuManager), add your recipes, then assign it to `KitchenManager.Menu`.

## Default Recipes

| Item | Quality | Difficulty | Prep Time | Price |
|---|---|---|---|---|
| Burger | 2 | 1 | 15s | 10 |
| Salad | 3 | 2 | 10s | 10 |
| Pasta | 4 | 3 | 25s | 10 |
| Pizza | 4 | 4 | 30s | 10 |
| Steak | 5 | 5 | 45s | 10 |

## Custom Recipes

Create a `RecipeResource` resource per item, set Name/BaseQuality/Difficulty/BasePreparationTime/Price, add to a `MenuManager` resource's `Recipes` array. Price defaults to 10 if not explicitly set.

## Customer Data Updates

`CustomerData` (assigned to each Visitor) now includes two new exports for the post-service eating phase:
- `MinEatTime`: Minimum duration (seconds) a visitor takes to eat after receiving their order (default 30s)
- `MaxEatTime`: Maximum duration (seconds) a visitor takes to eat after receiving their order (default 90s)

A random eat time is selected between these two values when the visitor enters the Eating state.

## How It Works

1. Visitor sits down → places random order from menu
2. KitchenManager enqueues order in FIFO queue
3. First idle Chef picks it up, starts cooking timer
4. Cook time = `Recipe.BasePreparationTime / Chef.Speed`
5. Chef finishes → `CookedItem` generated via formula:
   - `skillGap = Chef.Experience - Recipe.Difficulty`
   - `finalQuality = clamp(Recipe.BaseQuality + skillGap * 0.25 + Chef.Presentation * 0.1 + variance, 1, 5)`
6. Order fires `OrderCompleted` signal → visitor gets patience bonus (+15s default)
7. Waiting state stops patience timer, hides patience indicator (no longer relevant post-service)
8. Visitor enters **Eating** state: random duration between `CustomerData.MinEatTime` (default 30s) and `MaxEatTime` (default 90s)
9. Post-eating:
   - Visitor pays `Recipe.Price` (automatically tracked in `VisitorManager.TotalEarnings`)
   - Visitor submits rating based on `CookedItem.FinalQuality` (emits `VisitorManager.RatingSubmitted` signal)
10. Visitor stands up, leaves the restaurant, visit completes
11. KitchenManager assigns next pending order to now-idle chef

## Multi-Chef Behavior

- Multiple chefs cook orders in parallel
- Each picks the next pending order when idle
- Orders remain FIFO across all chefs