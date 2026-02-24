.class public final Lcom/kamcord/a/a/f/KC_c;
.super Lcom/kamcord/a/a/f/KC_a;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/a/a/f/KC_a;-><init>()V

    return-void
.end method


# virtual methods
.method public final a([B)Ljava/lang/String;
    .locals 1

    const/4 v0, 0x0

    invoke-static {p1, v0}, Landroid/util/Base64;->encodeToString([BI)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public final b()Ljava/lang/String;
    .locals 1

    const-string v0, "DatatypeConverter"

    return-object v0
.end method
