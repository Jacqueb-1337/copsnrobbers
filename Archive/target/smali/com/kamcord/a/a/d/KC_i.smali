.class public final enum Lcom/kamcord/a/a/d/KC_i;
.super Ljava/lang/Enum;


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "Ljava/lang/Enum",
        "<",
        "Lcom/kamcord/a/a/d/KC_i;",
        ">;"
    }
.end annotation


# static fields
.field public static final enum a:Lcom/kamcord/a/a/d/KC_i;

.field public static final enum b:Lcom/kamcord/a/a/d/KC_i;

.field private static final synthetic c:[Lcom/kamcord/a/a/d/KC_i;


# direct methods
.method static constructor <clinit>()V
    .locals 4

    const/4 v3, 0x1

    const/4 v2, 0x0

    new-instance v0, Lcom/kamcord/a/a/d/KC_i;

    const-string v1, "Header"

    invoke-direct {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_i;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/a/a/d/KC_i;->a:Lcom/kamcord/a/a/d/KC_i;

    new-instance v0, Lcom/kamcord/a/a/d/KC_i;

    const-string v1, "QueryString"

    invoke-direct {v0, v1, v3}, Lcom/kamcord/a/a/d/KC_i;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/a/a/d/KC_i;->b:Lcom/kamcord/a/a/d/KC_i;

    const/4 v0, 0x2

    new-array v0, v0, [Lcom/kamcord/a/a/d/KC_i;

    sget-object v1, Lcom/kamcord/a/a/d/KC_i;->a:Lcom/kamcord/a/a/d/KC_i;

    aput-object v1, v0, v2

    sget-object v1, Lcom/kamcord/a/a/d/KC_i;->b:Lcom/kamcord/a/a/d/KC_i;

    aput-object v1, v0, v3

    sput-object v0, Lcom/kamcord/a/a/d/KC_i;->c:[Lcom/kamcord/a/a/d/KC_i;

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

.method public static valueOf(Ljava/lang/String;)Lcom/kamcord/a/a/d/KC_i;
    .locals 1

    const-class v0, Lcom/kamcord/a/a/d/KC_i;

    invoke-static {v0, p0}, Ljava/lang/Enum;->valueOf(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/Enum;

    move-result-object v0

    check-cast v0, Lcom/kamcord/a/a/d/KC_i;

    return-object v0
.end method

.method public static values()[Lcom/kamcord/a/a/d/KC_i;
    .locals 1

    sget-object v0, Lcom/kamcord/a/a/d/KC_i;->c:[Lcom/kamcord/a/a/d/KC_i;

    invoke-virtual {v0}, [Lcom/kamcord/a/a/d/KC_i;->clone()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, [Lcom/kamcord/a/a/d/KC_i;

    return-object v0
.end method
