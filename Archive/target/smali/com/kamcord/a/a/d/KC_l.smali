.class public final Lcom/kamcord/a/a/d/KC_l;
.super Ljava/lang/Object;


# instance fields
.field private final a:Ljava/lang/String;


# direct methods
.method public constructor <init>(Ljava/lang/String;)V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const-string v0, "Must provide a valid string as verifier"

    invoke-static {p1, v0}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/Object;Ljava/lang/String;)V

    iput-object p1, p0, Lcom/kamcord/a/a/d/KC_l;->a:Ljava/lang/String;

    return-void
.end method


# virtual methods
.method public final a()Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_l;->a:Ljava/lang/String;

    return-object v0
.end method
