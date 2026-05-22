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

### 4. MenuManager (optional)

To override the 5 default recipes, create a `MenuManager` resource in the Filesystem dock (right-click → New Resource → MenuManager), add your recipes, then assign it to `KitchenManager.Menu`.

## Default Recipes

| Item | Quality | Difficulty | Prep Time |
|---|---|---|---|
| Burger | 2 | 1 | 15s |
| Salad | 3 | 2 | 10s |
| Pasta | 4 | 3 | 25s |
| Pizza | 4 | 4 | 30s |
| Steak | 5 | 5 | 45s |

## Custom Recipes

Create a `RecipeResource` resource per item, set Name/BaseQuality/Difficulty/BasePreparationTime, add to a `MenuManager` resource's `Recipes` array.

## How It Works

1. Visitor sits down → places random order from menu
2. KitchenManager enqueues order in FIFO queue
3. First idle Chef picks it up, starts cooking timer
4. Cook time = `Recipe.BasePreparationTime / Chef.Speed`
5. Chef finishes → `CookedItem` generated via formula:
   - `skillGap = Chef.Experience - Recipe.Difficulty`
   - `finalQuality = clamp(Recipe.BaseQuality + skillGap * 0.25 + Chef.Presentation * 0.1 + variance, 1, 5)`
6. Order fires `OrderCompleted` signal → visitor gets patience bonus (+15s default)
7. KitchenManager assigns next pending order to now-idle chef

## Multi-Chef Behavior

- Multiple chefs cook orders in parallel
- Each picks the next pending order when idle
- Orders remain FIFO across all chefs