.class final enum Lcom/kamcord/android/KC_Y$KC_c;
.super Ljava/lang/Enum;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_Y;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x4018
    name = "KC_c"
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "Ljava/lang/Enum",
        "<",
        "Lcom/kamcord/android/KC_Y$KC_c;",
        ">;"
    }
.end annotation


# static fields
.field public static final enum a:Lcom/kamcord/android/KC_Y$KC_c;

.field public static final enum b:Lcom/kamcord/android/KC_Y$KC_c;

.field private static final synthetic c:[Lcom/kamcord/android/KC_Y$KC_c;


# direct methods
.method static constructor <clinit>()V
    .locals 4

    const/4 v3, 0x1

    const/4 v2, 0x0

    new-instance v0, Lcom/kamcord/android/KC_Y$KC_c;

    const-string v1, "VIDEO"

    invoke-direct {v0, v1, v2}, Lcom/kamcord/android/KC_Y$KC_c;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/android/KC_Y$KC_c;->a:Lcom/kamcord/android/KC_Y$KC_c;

    new-instance v0, Lcom/kamcord/android/KC_Y$KC_c;

    const-string v1, "VOICE"

    invoke-direct {v0, v1, v3}, Lcom/kamcord/android/KC_Y$KC_c;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/android/KC_Y$KC_c;->b:Lcom/kamcord/android/KC_Y$KC_c;

    const/4 v0, 0x2

    new-array v0, v0, [Lcom/kamcord/android/KC_Y$KC_c;

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_c;->a:Lcom/kamcord/android/KC_Y$KC_c;

    aput-object v1, v0, v2

    sget-object v1, Lcom/kamcord/android/KC_Y$KC_c;->b:Lcom/kamcord/android/KC_Y$KC_c;

    aput-object v1, v0, v3

    sput-object v0, Lcom/kamcord/android/KC_Y$KC_c;->c:[Lcom/kamcord/android/KC_Y$KC_c;

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

.method public static valueOf(Ljava/lang/String;)Lcom/kamcord/android/KC_Y$KC_c;
    .locals 1

    const-class v0, Lcom/kamcord/android/KC_Y$KC_c;

    invoke-static {v0, p0}, Ljava/lang/Enum;->valueOf(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/Enum;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_Y$KC_c;

    return-object v0
.end method

.method public static values()[Lcom/kamcord/android/KC_Y$KC_c;
    .locals 1

    sget-object v0, Lcom/kamcord/android/KC_Y$KC_c;->c:[Lcom/kamcord/android/KC_Y$KC_c;

    invoke-virtual {v0}, [Lcom/kamcord/android/KC_Y$KC_c;->clone()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, [Lcom/kamcord/android/KC_Y$KC_c;

    return-object v0
.end method
