namespace AFKRaiders.Server.AI;

using AFKRaiders.Server.Domain;

/// <summary>
/// Fast, deterministic template narrative generator fulfilling the primary baseline and fallback contract.
/// Emits 3-bullet deadpan parody debriefs conforming to docs/lore/content-guidelines/TONE_VOICE.md.
/// </summary>
public sealed class TemplateNarrativeGenerator : INarrativeGenerator
{
    public string Name => "TemplateNarrator";
    public bool IsAvailable => true;

    public AarDebriefView GenerateAarDebrief(AarDebriefContext context)
    {
        var bullets = CreateBullets(context);
        return new AarDebriefView(bullets, "ArchivalLog", DateTimeOffset.UtcNow);
    }

    public ValueTask<AarDebriefView> GenerateAarDebriefAsync(AarDebriefContext context, CancellationToken ct = default)
    {
        return ValueTask.FromResult(GenerateAarDebrief(context));
    }

    public ValueTask<string> GenerateEnrichedEventTextAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default)
    {
        var text = request.EventType switch
        {
            "AarDebrief" when request.EventPayload is AarDebriefContext ctx =>
                string.Join("\n", CreateBullets(ctx).Select(b => $"▸ {b}")),
            _ => RaiderCommsContent.Line(request.EventType,
                (request.EventPayload as AmbientCommsContext)?.Activity, request.EventSequence)
        };
        return ValueTask.FromResult(text);
    }

    public static IReadOnlyList<string> CreateBullets(AarDebriefContext context)
    {
        var list = new List<string>(3);

        if (string.Equals(context.Outcome, "KnockedOut", StringComparison.OrdinalIgnoreCase))
        {
            // Bullet 1: Cause of defeat & unscheduled structural stress test
            var defeatCause = !string.IsNullOrWhiteSpace(context.CauseOfDefeat)
                ? $"Cause of incapacitation: {context.CauseOfDefeat}."
                : "Operative suffered terminal loss of consciousness.";
            list.Add($"Operative {context.RaiderName} recovered by Desperanza extraction team face-down near warning sign. {defeatCause} Incident filed as an unscheduled structural stress test.");

            // Bullet 2: Lost haul & Secret Hidden Pocket salvage
            if (context.PocketSalvagedItem is not null)
            {
                list.Add($"Unpocketed salvage ({context.LostHaulValue} scrap) lost to surface bureaucracy. Secret Hidden Pocket salvage: 1x {context.PocketSalvagedItem.Name} (Val: {context.PocketSalvagedItem.Value}) successfully recovered to Stash.");
            }
            else
            {
                list.Add($"Total haul loss ({context.LostHaulValue} scrap) incurred due to terminal incapacitation. Secret Hidden Pocket was empty. Regret declared non-refundable.");
            }

            // Bullet 3: Medbay invoice & RETURNED MOSTLY stamp
            list.Add("Medbay stamped recovery manifest 'RETURNED MOSTLY'. Emotional warranty claims were summarily rejected by Central Administration.");

            return list;
        }

        // Bullet 1: Haul evaluation & object overvaluation / tactical rationalization
        if (context.SecuredItemCount > 0 && context.TopItem is not null)
        {
            list.Add($"Secured {context.SecuredItemCount} item(s) valued at {context.TotalValue} scrap, led by {context.TopItem.Name}. Operative classified cargo as non-negotiable strategic infrastructure.");
        }
        else
        {
            list.Add("Returned with zero salvage. Operative filed paperwork classifying empty pockets as an advanced weight-reduction reconnaissance doctrine.");
        }

        // Bullet 2: Encounter / combat / hazard survival understatement or machine bureaucracy
        if (string.Equals(context.RobotEncounterResolution, "Defeated", StringComparison.OrdinalIgnoreCase))
        {
            list.Add("Roomba Prime was disabled at Service Depot. Operative filed victory under 'routine administrative pest control'.");
        }
        else if (string.Equals(context.RobotEncounterResolution, "Escaped", StringComparison.OrdinalIgnoreCase))
        {
            list.Add("Roomba Prime escaped after 3 rounds. Operative claims the machine retreated out of overwhelming tactical boredom.");
        }
        else if (context.DamageTaken > 0)
        {
            list.Add($"Sustained {context.DamageTaken} combat damage during operation. Operative describes blood loss as an authorized coolant flush.");
        }
        else
        {
            list.Add("Zero structural damage sustained. Operative expresses deep suspicion regarding the unprecedented lack of catastrophic failure.");
        }

        // Bullet 3: Extraction / greed / handler relation
        if (context.FinalGreed >= 30)
        {
            list.Add($"Greed factor peaked at {context.FinalGreed}. Mission closed with operative reluctantly boarding the return shuttle under mild Handler protest.");
        }
        else
        {
            list.Add("Extraction executed within acceptable safety margins. Dignity remains unaccounted for in official Desperanza records.");
        }

        return list;
    }

    public ItemAppraisalView GenerateItemAppraisal(ItemAppraisalContext context)
    {
        var (classification, appraisal) = CreateAppraisal(context);
        return new ItemAppraisalView(
            CatalogId: context.CatalogId,
            ItemName: context.ItemName,
            Appraisal: appraisal,
            StrategicClassification: classification,
            Source: "ArchivalLog",
            GeneratedAt: DateTimeOffset.UtcNow
        );
    }

    public ValueTask<ItemAppraisalView> GenerateItemAppraisalAsync(ItemAppraisalContext context, CancellationToken ct = default)
    {
        return ValueTask.FromResult(GenerateItemAppraisal(context));
    }

    public static (string Classification, string Appraisal) CreateAppraisal(ItemAppraisalContext context)
    {
        var id = context.CatalogId.ToLowerInvariant();
        var name = context.ItemName;

        return id switch
        {
            "tea_kettle" => (
                "Steam-Pressurized Offensive Whistling Apparatus",
                "Desperanza Ordnance certifies this standard-issue kettle as capable of severe auditory intimidation and psychological battlefield disruption."
            ),
            "aspperigo" => (
                "High-Vibration Kinetic Persuasion Carbine",
                "Armory records confirm Eddie's beloved rifle functions primarily by vibrating the operator into a state of heightened tactical clarity."
            ),
            "vernerider" => (
                "Calibrated Ballistic Dispute Resolution Tool",
                "Desperanza Munitions notes that phonetic arguments over this weapon's name have resolved more disputes than its actual ammunition."
            ),
            "temptrest_ar" => (
                "Flagship Tactical Prestige Multiplier",
                "The undisputed crown jewel of surface salvage. Possessing this weapon instantly elevates an operative from 'reckless vagrant' to 'strategic regional power'."
            ),
            "bandage_white" or "bandage_green" or "bandage_blue" or "bandage_purple" => (
                "Bio-Adhesive Emergency Morale Patch",
                "Desperanza Medical confirms this adhesive strip converts arterial panic into a manageable administrative inconvenience."
            ),
            "panic_paddles" => (
                "Non-Voluntary Resuscitation Agreement",
                "Two heavily scorched metal plates. Specifically designed to jolt fallen operatives back into consciousness so they can complete pending liability waivers."
            ),
            "fizz_cell" or "duct_taped_capacitor" or "panic_capacitor" => (
                "High-Risk Portable Energy Displacement Core",
                "Desperanza Power Bureau logs caution that the brave snapping noises indicate either impending shield recharge or an exciting chemical release."
            ),
            _ => CreateGenericAppraisal(context)
        };
    }

    private static (string Classification, string Appraisal) CreateGenericAppraisal(ItemAppraisalContext context)
    {
        var name = context.ItemName;
        var value = context.ItemValue;
        var hash = Math.Abs(name.GetHashCode(StringComparison.OrdinalIgnoreCase)) % 4;

        return hash switch
        {
            0 => (
                "Pre-Cataclysm Sub-Surface Computing Substrate",
                $"Operative {context.RaiderName} insists this {name} (est. {value} scrap) holds encrypted firmware vital to regional municipal sovereignty."
            ),
            1 => (
                "Heavy-Duty Ergonomic Kinetic Implement",
                $"Armory examiners note that with sufficient optimism and velocity, this {name} performs identically to Grade-4 military breaching hardware."
            ),
            2 => (
                "Tactical Domestic Life-Support Component",
                $"Desperanza Salvage Bureau has provisionally re-indexed this {name} under 'Non-Negotiable Strategic Infrastructure' to justify transport costs."
            ),
            _ => (
                "High-Yield Electromagnetic Sentiment Reservoir",
                $"Despite apparent corrosion, operative {context.RaiderName} testified that discarding this {name} would trigger an immediate structural morale collapse."
            )
        };
    }
}
