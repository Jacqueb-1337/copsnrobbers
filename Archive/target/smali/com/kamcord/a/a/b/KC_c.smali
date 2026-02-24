.class public final Lcom/kamcord/a/a/b/KC_c;
.super Lcom/kamcord/a/a/b/KC_b;


# static fields
.field private static final serialVersionUID:J = 0x18389511fdda10dfL


# direct methods
.method public constructor <init>(Lcom/kamcord/a/a/d/KC_c;)V
    .locals 3

    const-string v0, "Could not find oauth parameters in request: %s. OAuth parameters must be specified with the addOAuthParameter() method"

    const/4 v1, 0x1

    new-array v1, v1, [Ljava/lang/Object;

    const/4 v2, 0x0

    aput-object p1, v1, v2

    invoke-static {v0, v1}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v0

    invoke-direct {p0, v0}, Lcom/kamcord/a/a/b/KC_b;-><init>(Ljava/lang/String;)V

    return-void
.end method
