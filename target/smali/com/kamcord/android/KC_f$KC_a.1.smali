.class final enum Lcom/kamcord/android/KC_f$KC_a;
.super Ljava/lang/Enum;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_f;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x4018
    name = "KC_a"
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "Ljava/lang/Enum",
        "<",
        "Lcom/kamcord/android/KC_f$KC_a;",
        ">;"
    }
.end annotation


# static fields
.field public static final enum a:Lcom/kamcord/android/KC_f$KC_a;

.field public static final enum b:Lcom/kamcord/android/KC_f$KC_a;

.field private static final synthetic c:[Lcom/kamcord/android/KC_f$KC_a;


# direct methods
.method static constructor <clinit>()V
    .locals 4

    const/4 v3, 0x1

    const/4 v2, 0x0

    new-instance v0, Lcom/kamcord/android/KC_f$KC_a;

    const-string v1, "EMAIL"

    invoke-direct {v0, v1, v2}, Lcom/kamcord/android/KC_f$KC_a;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/android/KC_f$KC_a;->a:Lcom/kamcord/android/KC_f$KC_a;

    new-instance v0, Lcom/kamcord/android/KC_f$KC_a;

    const-string v1, "PASSWORD"

    invoke-direct {v0, v1, v3}, Lcom/kamcord/android/KC_f$KC_a;-><init>(Ljava/lang/String;I)V

    sput-object v0, Lcom/kamcord/android/KC_f$KC_a;->b:Lcom/kamcord/android/KC_f$KC_a;

    const/4 v0, 0x2

    new-array v0, v0, [Lcom/kamcord/android/KC_f$KC_a;

    sget-object v1, Lcom/kamcord/android/KC_f$KC_a;->a:Lcom/kamcord/android/KC_f$KC_a;

    aput-object v1, v0, v2

    sget-object v1, Lcom/kamcord/android/KC_f$KC_a;->b:Lcom/kamcord/android/KC_f$KC_a;

    aput-object v1, v0, v3

    sput-object v0, Lcom/kamcord/android/KC_f$KC_a;->c:[Lcom/kamcord/android/KC_f$KC_a;

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

.method public static valueOf(Ljava/lang/String;)Lcom/kamcord/android/KC_f$KC_a;
    .locals 1

    const-class v0, Lcom/kamcord/android/KC_f$KC_a;

    invoke-static {v0, p0}, Ljava/lang/Enum;->valueOf(Ljava/lang/Class;Ljava/lang/String;)Ljava/lang/Enum;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KC_f$KC_a;

    return-object v0
.end method

.method public static values()[Lcom/kamcord/android/KC_f$KC_a;
    .locals 1

    sget-object v0, Lcom/kamcord/android/KC_f$KC_a;->c:[Lcom/kamcord/android/KC_f$KC_a;

    invoke-virtual {v0}, [Lcom/kamcord/android/KC_f$KC_a;->clone()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, [Lcom/kamcord/android/KC_f$KC_a;

    return-object v0
.end method
