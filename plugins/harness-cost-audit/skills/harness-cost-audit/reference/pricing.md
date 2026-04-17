# Claude API Pricing Table

**Version marker:** 2026-04-17

Per 1M tokens, USD. Used by `scripts/analyze_transcripts.py` and the statusline
turn-cost helper.

| Model family | input | output | cache 5m write | cache 1h write | cache read |
|--------------|------:|-------:|---------------:|---------------:|-----------:|
| Opus 4.x     |  $15  |  $75   |   $18.75       |    $30         |   $1.50    |
| Sonnet 4.x   |   $3  |  $15   |    $3.75       |     $6         |   $0.30    |
| Haiku 4.x    |   $1  |   $5   |    $1.25       |     $2         |   $0.10    |

## Notes

- **Cache write multipliers**: 5m is 1.25× input, 1h is 2× input.
- **Cache read**: 0.1× input (same multiplier across families).
- **Model detection**: scripts match `opus`/`sonnet`/`haiku` as substrings in
  `message.model`. Conservative default on unknown models: Opus pricing.

## Update procedure

When Anthropic publishes new rates:

1. Update the table above.
2. Update `PRICES` dict in `scripts/analyze_transcripts.py`.
3. Update the case statement in `~/.claude/statusline.sh` (if the user
   installed the per-turn cost indicator).
4. Bump `PRICE_VERSION` constant (one place in the script).
5. Re-run `analyze_transcripts.py` on recent sessions — prior-month numbers
   will be repriced under new rates, which is correct for forward-looking
   projections but NOT for historical billing reconciliation. If
   reconciliation matters, archive the previous pricing.md as
   `reference/pricing-<date>.md` before updating.

## Source of truth

- Anthropic pricing page (check the official Anthropic docs site for the
  current rates — URL omitted to avoid linkrot).
- Model cards for the specific variant in use.
