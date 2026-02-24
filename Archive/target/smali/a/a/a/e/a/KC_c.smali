.class public final La/a/a/e/a/KC_c;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/e/a/KC_c$KC_b;,
        La/a/a/e/a/KC_c$KC_a;
    }
.end annotation


# static fields
.field private static final a:La/a/a/e/a/KC_b;


# instance fields
.field private final b:Ljava/lang/Object;


# direct methods
.method static constructor <clinit>()V
    .locals 2

    .prologue
    .line 139
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x13

    if-lt v0, v1, :cond_0

    .line 140
    new-instance v0, La/a/a/e/a/KC_c$KC_b;

    invoke-direct {v0}, La/a/a/e/a/KC_c$KC_b;-><init>()V

    sput-object v0, La/a/a/e/a/KC_c;->a:La/a/a/e/a/KC_b;

    .line 146
    :goto_0
    return-void

    .line 141
    :cond_0
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x10

    if-lt v0, v1, :cond_1

    .line 142
    new-instance v0, La/a/a/e/a/KC_c$KC_a;

    invoke-direct {v0}, La/a/a/e/a/KC_c$KC_a;-><init>()V

    sput-object v0, La/a/a/e/a/KC_c;->a:La/a/a/e/a/KC_b;

    goto :goto_0

    .line 144
    :cond_1
    new-instance v0, La/a/a/e/a/KC_b;

    invoke-direct {v0}, La/a/a/e/a/KC_b;-><init>()V

    sput-object v0, La/a/a/e/a/KC_c;->a:La/a/a/e/a/KC_b;

    goto :goto_0
.end method

.method public constructor <init>()V
    .locals 1

    .prologue
    .line 151
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 152
    sget-object v0, La/a/a/e/a/KC_c;->a:La/a/a/e/a/KC_b;

    invoke-virtual {v0, p0}, La/a/a/e/a/KC_b;->a(La/a/a/e/a/KC_c;)Ljava/lang/Object;

    move-result-object v0

    iput-object v0, p0, La/a/a/e/a/KC_c;->b:Ljava/lang/Object;

    .line 153
    return-void
.end method

.method public constructor <init>(Ljava/lang/Object;)V
    .locals 0

    .prologue
    .line 161
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 162
    iput-object p1, p0, La/a/a/e/a/KC_c;->b:Ljava/lang/Object;

    .line 163
    return-void
.end method

.method public static b()La/a/a/e/a/KC_a;
    .locals 1

    .prologue
    .line 195
    const/4 v0, 0x0

    return-object v0
.end method

.method public static c()Z
    .locals 1

    .prologue
    .line 212
    const/4 v0, 0x0

    return v0
.end method

.method public static d()Ljava/util/List;
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()",
            "Ljava/util/List",
            "<",
            "La/a/a/e/a/KC_a;",
            ">;"
        }
    .end annotation

    .prologue
    .line 231
    const/4 v0, 0x0

    return-object v0
.end method

.method public static e()La/a/a/e/a/KC_a;
    .locals 1

    .prologue
    .line 246
    const/4 v0, 0x0

    return-object v0
.end method


# virtual methods
.method public final a()Ljava/lang/Object;
    .locals 1

    .prologue
    .line 169
    iget-object v0, p0, La/a/a/e/a/KC_c;->b:Ljava/lang/Object;

    return-object v0
.end method
