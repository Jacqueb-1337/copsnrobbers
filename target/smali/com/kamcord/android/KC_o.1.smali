.class final enum Lcom/kamcord/android/KC_o;
.super Ljava/lang/Enum;


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "Ljava/lang/Enum",
        "<",
        "Lcom/kamcord/android/KC_o;",
        ">;"
    }
.end annotation


# static fields
.field public static final enum a:Lcom/kamcord/android/KC_o;

.field public static final enum b:Lcom/kamcord/android/KC_o;

.field public static final enum c:Lcom/kamcord/android/KC_o;

.field public static final enum d:Lcom/kamcord/android/KC_o;

.field private static final synthetic e:[Lcom/kamcord/android/KC_o;


# direct methods
.method static constructor <clinit>()V
    .locals 6

    const/4 v5, 0x3

    const/4 v4, 0x2

    const/4 v3, 0x1

    const/4 v2, 0x0

    new-instance v0, Lcom/kamcord/android/KC_o;

    const-string v1, "IDLE"

    invoke-direct {v0, v1, v2}, Lcom/kamcord/android/KC_o;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    new-instance v0, Lcom/kamcord/android/KC_o;

    const-string v1, "RECORDING"

    invoke-direct {v0, v1, v3}, Lcom/kamcord/android/KC_o;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/android/KC_o;->b:Lcom/kamcord/android/KC_o;

    new-instance v0, Lcom/kamcord/android/KC_o;

    const-string v1, "PAUSED"

    invoke-direct {v0, v1, v4}, Lcom/kamcord/android/KC_o;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/android/KC_o;->c:Lcom/kamcord/android/KC_o;

    new-instance v0, Lcom/kamcord/android/KC_o;

    const-string v1, "VIEW_UP"

    invoke-direct {v0, v1, v5}, Lcom/kamcord/android/KC_o;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/android/KC_o;->d:Lcom/kamcord/android/KC_o;

    const/4 v0, 0x4

    new-array v0, v0, [Lcom/kamcord/android/KC_o;

    sget-object v1, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    aput-object v1, v0, v2

    sget-object v1, Lcom/kamcord/android/KC_o;->b:Lcom/kamcord/android/KC_o;

    aput-object v1, v0, v3

    sget-object v1, Lcom/kamcord/android/KC_o;->c:Lcom/kamcord/android/KC_o;

    aput-object v1, v0, v4

    sget-object v1, Lcom/kamcord/android/KC_o;->d:Lcom/kamcord/android/KC_o;

    aput-object v1, v0, v5

    sput-object v0, Lcom/kamcord/android/KC_o;->e:[Lcom/kamcord/android/KC_o;

    return-void
.end method

.method private constructor <init>(Ljava/lang/String;I)V
    .locals 0
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()V"
        }
    .end annotation

    invoke-direct {p0, p1, p2}, Ljava/lang/Enum;-><init>(Ljava/lang/String;I)V

    return-void
.end method

.method public static valueOf(Ljava/lang/String;)Lcom/kamcord/android/KC_o;
    .locals 1

    const-class v0, Lcom/kamcord/android/KC_o;

    invoke-static {v0, p0}, Ljava/lang/Enum;->valueOf(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/Enum;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_o;

    return-object v0
.end method

.method public static values()[Lcom/kamcord/android/KC_o;
    .locals 1

    sget-object v0, Lcom/kamcord/android/KC_o;->e:[Lcom/kamcord/android/KC_o;

    invoke-virtual {v0}, [Lcom/kamcord/android/KC_o;->clone()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, [Lcom/kamcord/android/KC_o;

    return-object v0
.end method
